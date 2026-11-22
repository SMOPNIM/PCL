namespace PCL;

/// <summary>
/// 获取搜索文本的相似度。
/// </summary>
internal static class SearchSimilarityHelper {
    /// <summary>
    /// 获取搜索文本的相似度。
    /// </summary>
    /// <param name="Source">被搜索的长内容。</param>
    /// <param name="Query">用户输入的搜索文本。</param>
    public static double SearchSimilarity(string? Source, string? Query) {
        if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Query)) return 0;
        var qp = 0;
        double lenSum = 0;
        var str = new StringBuilder(Source!.Length);
        str.Append(Source.Lower().Replace(" ", ""));
        Query = Query!.Lower().Replace(" ", "");
        var sourceLength = str.Length; //用于计算最后因数的长度缓存
        var queryLength = Query.Length;
        if (queryLength == 0) return 0;
        while (qp < queryLength) {
            //对 qp 作为开始位置计算
            var sp = 0;
            var lenMax = 0;
            var spMax = 0;
            var currentSourceLength = str.Length;
            while (sp < currentSourceLength) {
                var len = 0;
                while (qp + len < queryLength && sp + len < currentSourceLength && str[sp + len] == Query[qp + len]) {
                    len++;
                }
                //存储 len
                if (len > lenMax) {
                    lenMax = len;
                    spMax = sp;
                }
                //根据结果增加 sp
                sp += len > 0 ? len : 1;
            }
            if (lenMax > 0) {
                str.Remove(spMax, lenMax); //将源中的对应字段移除
                //存储 lenSum
                var IncWeight = Math.Pow(1.4, 3 + lenMax) - 3.6; //根据长度加成
                IncWeight *= 1 + 0.3 * Math.Max(0, 3 - Math.Abs(qp - spMax)); //根据位置加成
                lenSum += IncWeight;
            }
            qp += lenMax > 0 ? lenMax : 1;
        }
        //计算结果：重复字段量 × 源长度影响比例
        return (lenSum / queryLength) * (3 / Math.Sqrt(sourceLength + 15)) * (queryLength <= 2 ? 3 - queryLength : 1);
    }

    /// <summary>
    /// 获取多段文本加权后的相似度。
    /// </summary>
    public static double SearchSimilarityWeighted(List<SearchSource> Source, string Query) {
        var TotalWeight = 0d;
        var Sum = 0d;
        foreach (var Pair in Source) {
            if (Pair.Aliases.Any()) {
                Sum += Pair.Aliases.Max(a => SearchSimilarity(a, Query)) * Pair.Weight;
            }
            TotalWeight += Pair.Weight;
        }
        return Sum / TotalWeight;
    }
}

/// <summary>
/// 用于搜索的项目。
/// </summary>
public class SearchEntry<T> {
    /// <summary>
    /// 该项目对应的源数据。
    /// </summary>
    public T? Item { get; set; } = default!;
    /// <summary>
    /// 该项目用于搜索的文本源。
    /// 在搜索时，会对每个文本源单独加权，但单个文本源内的多个别名只取最高的一个的相似度。
    /// </summary>
    public List<SearchSource> SearchSource { get; set; } = null!;
    /// <summary>
    /// 相似度。
    /// </summary>
    public double Similarity { get; set; }
    /// <summary>
    /// 是否完全匹配。
    /// </summary>
    public bool AbsoluteRight { get; set; }
    public override string ToString() {
        return Math.Round(Similarity, 3) + " - " + Item.ToString();
    }
}

/// <summary>
/// 单个用于搜索的文本源。
/// </summary>
public class SearchSource {
    public string[] Aliases { get; set; } = null!;
    public double Weight { get; set; }
    public SearchSource(string[] Aliases, double Weight = 1) {
        this.Aliases = Aliases;
        this.Weight = Weight;
    }
    public SearchSource(string Text, double Weight = 1) {
        Aliases = new[] { Text };
        this.Weight = Weight;
    }
}

/// <summary>
/// 多段文本加权搜索。
/// </summary>
public static class TextSearch {
    /// <summary>
    /// 进行多段文本加权搜索，获取相似度较高的数项结果。
    /// 在搜索时，会对每个文本源单独加权，但单个文本源内的多个别名只取最高的一个的相似度。
    /// 这会修改 Entries 中每项的 Similarity 与 AbsoluteRight 字段。
    /// </summary>
    /// <param name="MaxBlurCount">返回的最大模糊结果数。</param>
    /// <param name="MinBlurSimilarity">返回结果要求的最低相似度。</param>
    public static List<SearchEntry<T>> Search<T>(List<SearchEntry<T>> Entries, string Query, int MaxBlurCount = 5, double MinBlurSimilarity = 0.1) {
        //初始化
        var ResultList = new List<SearchEntry<T>>();
        if (!Entries.Any()) return ResultList;
        //进行搜索，获取相似信息
        var QueryParts = Query.Split(' ').Where(s => s.Length > 0).ToArray();
        var Candidates = new List<SearchEntry<T>>();
        foreach (var Entry in Entries) {
            Entry.Similarity = SearchSimilarityHelper.SearchSimilarityWeighted(Entry.SearchSource, Query);
            Entry.AbsoluteRight =
                QueryParts.All( //对于按空格分割的每一段
                QueryPart => Entry.SearchSource.Any( //若与任意一个搜索源完全匹配，则标记为完全匹配项
                Source => Source.Aliases.Any(
                Alias => Alias.Replace(" ", "").ContainsIgnoreCase(QueryPart))));
            if (Entry.AbsoluteRight || Entry.Similarity >= MinBlurSimilarity) Candidates.Add(Entry);
        }
        //按照相似度进行排序（选择排序，与 ModBase.SortByComparison 行为一致）
        var SortedCandidates = new List<SearchEntry<T>>();
        while (Candidates.Any()) {
            var Highest = Candidates[0];
            for (var i = 1; i < Candidates.Count; i++) {
                if (Cmp(Candidates[i], Highest)) Highest = Candidates[i];
            }
            Candidates.Remove(Highest);
            SortedCandidates.Add(Highest);
        }
        Candidates = SortedCandidates;
        static bool Cmp(SearchEntry<T> Left, SearchEntry<T> Right) =>
            Left.AbsoluteRight != Right.AbsoluteRight ? Left.AbsoluteRight : Left.Similarity > Right.Similarity;
        //返回结果
        var BlurCount = 0;
        foreach (var Entry in Candidates) {
            if (Entry.AbsoluteRight) {
                ResultList.Add(Entry); //完全匹配，直接加入
            } else {
                if (BlurCount == MaxBlurCount) break;
                ResultList.Add(Entry);
                BlurCount++; //模糊结果计数
            }
        }
        return ResultList;
    }
}
