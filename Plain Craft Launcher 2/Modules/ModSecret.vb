'由于包含加解密等安全信息，本文件中的部分代码已被删除

Friend Module ModSecret

    '标注 PCL 的不同分支，仅用于替换标记
    Public Const VersionBranchMain As String = "OpenSource"
    '使用与官方正式版相同的注册表根目录，使已解锁主题、土豆码等数据与官方版互通
    Public Const RegFolder As String = "PCL"
    '用于微软登录的 ClientId
    Public OAuthClientId As String = If(Environment.GetEnvironmentVariable("PCL_MS_CLIENT_ID"), "")
    'CurseForge API Key
    Public CurseForgeAPIKey As String = If(Environment.GetEnvironmentVariable("PCL_CURSEFORGE_API_KEY"), "")
    '用于匿名数据收集的腾讯云日志服务上报 URL，形如 https://{region}.cls.tencentcs.com/track?topic_id={topic_id}
    Public Const ClsBaseUrl As String = ""

#Region "网络鉴权"

    Friend Function SecretCdnSign(UrlWithMark As String) As String
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' 设置 Headers 的 UA、Referer。
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Req As HttpRequestMessage, Optional SimulateBrowserHeaders As Boolean = False)
        If Not Req.Headers.UserAgent.Any Then
            If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
                Req.Headers.Add("User-Agent", "LogStatistic")  '#4951
            ElseIf SimulateBrowserHeaders Then
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36")
            Else
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)}")
            End If
        End If
        If Not SimulateBrowserHeaders Then Req.Headers.Add("Referer", $"http://{VersionCode}.open.pcl2.server/")
        If Url.Contains("api.curseforge.com") OrElse Url.Contains("forgecdn.net") Then Req.Headers.Add("x-api-key", CurseForgeAPIKey)
    End Sub

#End Region

#Region "主题"

    'Debug 版下默认拥有全部赞助特性，主题与土豆码相关功能不受限制
    Public ReadOnly IsSponsoredFeaturesUnlocked As Boolean = BuildType = BuildTypes.Debug

    Private IsThemeColorfulLoaded As Boolean
    Private ColorHueDelta As Integer = 1

    Public Color1 As New MyColor(52, 61, 74)
    Public Color2 As New MyColor(11, 91, 203)
    Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As New MyColor(213, 230, 253)
    Public Color7 As New MyColor(222, 236, 253)
    Public Color8 As New MyColor(234, 242, 254)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGray1 As New MyColor(64, 64, 64)
    Public ColorGray2 As New MyColor(115, 115, 115)
    Public ColorGray3 As New MyColor(140, 140, 140)
    Public ColorGray4 As New MyColor(166, 166, 166)
    Public ColorGray5 As New MyColor(204, 204, 204)
    Public ColorGray6 As New MyColor(235, 235, 235)
    Public ColorGray7 As New MyColor(240, 240, 240)
    Public ColorGray8 As New MyColor(245, 245, 245)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As OneOf(Of Integer, Integer()) = 0
    Public ThemeDontClick As Integer = 0

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        Try
            If (ThemeNow = NewTheme AndAlso NewTheme >= 0) OrElse Not ThemeCheckOne(If(NewTheme >= 0, NewTheme, ThemeNow)) Then Return
            If NewTheme >= 0 Then ThemeNow = NewTheme

            Select Case ThemeNow
                Case 10
                    ColorHue = 5
                    ColorSat = 70
                    ColorLightAdjust = -5
                    ColorHueTopbarDelta = 0
                Case 9
                    ColorHue = 25
                    ColorSat = 75
                    ColorLightAdjust = 5
                    ColorHueTopbarDelta = 0
                Case 4
                    ColorHue = 35
                    ColorSat = 30
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = 0
                Case 8
                    ColorHue = 50
                    ColorSat = 65
                    ColorLightAdjust = 10
                    ColorHueTopbarDelta = 0
                Case 3
                    ColorHue = 75
                    ColorSat = 50
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = 0
                Case 2
                    ColorHue = 130
                    ColorSat = 40
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = New Integer() {5, -5, 5}
                Case 42
                    ColorHue = 225
                    ColorSat = 60
                    ColorLightAdjust = -15
                    ColorHueTopbarDelta = 0
                Case 1
                    ColorHue = 180
                    ColorSat = 50
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = 0
                Case 0
                    ColorHue = 210
                    ColorSat = 85
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = 0
                Case 5
                    ColorHue = 225
                    ColorSat = 10
                    ColorLightAdjust = -20
                    ColorHueTopbarDelta = 0
                Case 11
                    ColorHue = 230
                    ColorSat = 60
                    ColorLightAdjust = -5
                    ColorHueTopbarDelta = 0
                Case 7
                    ColorHue = 285
                    ColorSat = 50
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = 0
                Case 13
                    ColorHue = 290
                    ColorSat = 50
                    ColorLightAdjust = 0
                    ColorHueTopbarDelta = New Integer() {-30, 10, 50}
                Case 6
                    ColorHue = 345
                    ColorSat = 70
                    ColorLightAdjust = 15
                    ColorHueTopbarDelta = 0
                Case 12
                    If Not IsThemeColorfulLoaded Then
                        IsThemeColorfulLoaded = True
                        ColorHue = RandomInteger(0, 359)
                        ColorHueDelta = (RandomInteger(0, 1) * 2 - 1) * 4
                    End If
                    ColorHue = (ColorHue + ColorHueDelta) Mod 360
                    ColorSat = 45
                    ColorLightAdjust = 0
                Case 14
                    ColorHue = Settings.Get(Of Integer)("UiLauncherHue")
                    ColorSat = Settings.Get(Of Integer)("UiLauncherSat")
                    ColorLightAdjust = Settings.Get(Of Integer)("UiLauncherLight") - 20
                    ColorHueTopbarDelta = Settings.Get(Of Integer)("UiLauncherDelta") - 90
                Case Else
                    ColorHue = 210
                    ColorSat = 85
                    ColorLightAdjust = 0
            End Select

            Select Case ThemeDontClick
                Case 1
                    ColorLightAdjust = 999
                Case 2
                    ColorHue = RandomInteger(0, 359)
                    ColorSat = RandomInteger(40, 70)
                    ColorLightAdjust = RandomInteger(-20, 20)
            End Select

            Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.2, 25 + ColorLightAdjust * 0.3)
            Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 45 + ColorLightAdjust)
            Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 55 + ColorLightAdjust)
            Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 65 + ColorLightAdjust)
            Color5 = New MyColor().FromHSL2(ColorHue, ColorSat, 80 + ColorLightAdjust * 0.4)
            Color6 = New MyColor().FromHSL2(ColorHue, ColorSat, 91 + ColorLightAdjust * 0.1)
            Color7 = New MyColor().FromHSL2(ColorHue, ColorSat, 95)
            Color8 = New MyColor().FromHSL2(ColorHue, ColorSat, 97)
            ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2
            ColorBg1 = New MyColor(190, Color7)

            ColorSemiTransparent = New MyColor(1, Color8)
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(Color1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(Color2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(Color3)
            Application.Current.Resources("ColorBrush4") = New SolidColorBrush(Color4)
            Application.Current.Resources("ColorBrush5") = New SolidColorBrush(Color5)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(Color6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(Color7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(Color8)
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Application.Current.Resources("ColorObject1") = CType(Color1, Color)
            Application.Current.Resources("ColorObject2") = CType(Color2, Color)
            Application.Current.Resources("ColorObject3") = CType(Color3, Color)
            Application.Current.Resources("ColorObject4") = CType(Color4, Color)
            Application.Current.Resources("ColorObject5") = CType(Color5, Color)
            Application.Current.Resources("ColorObject6") = CType(Color6, Color)
            Application.Current.Resources("ColorObject7") = CType(Color7, Color)
            Application.Current.Resources("ColorObject8") = CType(Color8, Color)
            Application.Current.Resources("ColorObjectBg0") = CType(ColorBg0, Color)
            Application.Current.Resources("ColorObjectBg1") = CType(ColorBg1, Color)
            ThemeRefreshMain()
            If ThemeNow <> 12 AndAlso ThemeNow <> 14 AndAlso ThemeDontClick <> 2 Then
                Logger.Info($"刷新主题：{ThemeNow}")
            End If
        Catch ex As Exception
            Logger.Error(ex, "刷新主题颜色失败", LogBehavior.Toast)
        End Try
    End Sub
    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Return
            '顶部条背景
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            If ThemeNow = 5 Then
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 25)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 15)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 25)})
                FrmMain.PanTitle.Background = Brush
                FrmMain.PanTitle.Background.Freeze()
            ElseIf ThemeNow <> 12 AndAlso ThemeDontClick <> 2 Then
                Dim Deltas = ColorHueTopbarDelta.Switch(Function(d) New Integer() {-d, 0, d}, Function(d) d)
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + Deltas(0), ColorSat, 48 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + Deltas(1), ColorSat, 54 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + Deltas(2), ColorSat, 48 + ColorLightAdjust)})
                FrmMain.PanTitle.Background = Brush
                FrmMain.PanTitle.Background.Freeze()
            Else
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue - 21, ColorSat, 53 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.33, .Color = New MyColor().FromHSL2(ColorHue - 7, ColorSat, 47 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.67, .Color = New MyColor().FromHSL2(ColorHue + 7, ColorSat, 47 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + 21, ColorSat, 53 + ColorLightAdjust)})
                FrmMain.PanTitle.Background = Brush
            End If
            If ThemeNow >= 5 AndAlso ThemeNow <> 14 Then
                FrmMain.ImgTitle.Source = $"{PathImage}Themes\{ThemeNow}.png"
            Else
                FrmMain.ImgTitle.Source = Nothing
            End If
            FrmMain.ImgTitle.Opacity = If(ThemeNow = 13, 0.25, 0.5)
            '主页面背景
            If Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 15, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 15, ColorSat * 0.8, 91)})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = New MyColor(245, 245, 245)
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
        RunInUi(
        Sub()
            Try
                Dim Id = Settings.Get(Of Integer)("UiLauncherTheme")
                If Not ThemeCheckOne(Id) Then
                    Logger.Warn($"检测到尚未解锁的主题：{Id}，已重置为默认主题")
                    Settings.Set("UiLauncherTheme", 0)
                End If
                If EffectSetup AndAlso FrmSetupUI IsNot Nothing Then
                    Dim List As New List(Of Integer)
                    Dim i = 0
                    Do
                        If ThemeCheckOne(i) Then List.Add(i)
                        i += 1
                    Loop While i <= 14
                    List.Add(42)
                    AniControlEnabled += 1
                    If List.Contains(5) Then
                        FrmSetupUI.RadioLauncherTheme5.Opacity = 1.0
                        FrmSetupUI.LabLauncherTheme5Unlock.Visibility = Visibility.Collapsed
                        FrmSetupUI.RadioLauncherTheme5Gray.Visibility = Visibility.Collapsed
                    End If
                    If List.Contains(6) Then
                        FrmSetupUI.RadioLauncherTheme6.IsEnabled = True
                    End If
                    If List.Contains(7) Then
                        FrmSetupUI.RadioLauncherTheme7.IsEnabled = True
                    End If
                    If List.Contains(8) Then
                        FrmSetupUI.LabLauncherTheme8Copy.Visibility = Visibility.Collapsed
                        FrmSetupUI.RadioLauncherTheme8.IsEnabled = True
                    End If
                    If List.Contains(9) Then
                        FrmSetupUI.LabLauncherTheme9Copy.Visibility = Visibility.Collapsed
                        FrmSetupUI.RadioLauncherTheme9.IsEnabled = True
                    End If
                    If List.Contains(10) Then
                        FrmSetupUI.RadioLauncherTheme10.IsEnabled = True
                    End If
                    If List.Contains(11) Then
                        FrmSetupUI.LabLauncherTheme11Click.Visibility = Visibility.Collapsed
                        FrmSetupUI.RadioLauncherTheme11.IsEnabled = True
                    ElseIf FrmSetupUI IsNot Nothing AndAlso FrmSetupUI.RadioLauncherTheme11 IsNot Nothing AndAlso FrmSetupUI.RadioLauncherTheme11.IsLoaded Then
                        Dim Count = 0
                        For Each Item In List
                            If Item >= 5 AndAlso Item <= 14 Then Count += 1
                        Next
                        If Count < 3 Then
                            FrmSetupUI.LabLauncherTheme11Click.ToolTip = "需要解锁三个隐藏主题"
                        Else
                            FrmSetupUI.LabLauncherTheme11Click.ToolTip = "点击打开解密游戏入口"
                        End If
                    End If
                    If List.Contains(12) Then
                        FrmSetupUI.RadioLauncherTheme12.IsEnabled = True
                    End If
                    If List.Contains(13) Then
                        FrmSetupUI.RadioLauncherTheme13.IsEnabled = True
                    End If
                    If List.Contains(14) Then
                        FrmSetupUI.RadioLauncherTheme14.IsEnabled = True
                    End If
                    AniControlEnabled -= 1
                End If
            Catch ex As Exception
                If TypeOf (If(ex.InnerException, ex)) Is FormatException Then
                    Logger.Error(ex, "解锁的主题列表存档已损坏，主题解锁已被重置", LogBehavior.Alert)
                    Settings.Set("UiLauncherThemeHide2", "")
                Else
                    Logger.Error(ex, "检查主题失败", LogBehavior.Alert)
                End If
            End Try
        End Sub)
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        If IsSponsoredFeaturesUnlocked Then Return True
        If New Integer() {0, 1, 2, 3, 4, 42}.Contains(Id) Then Return True
        If PotatoFeatures.Contains($"Theme{Id}") Then Return True
        Select Case Id
            Case 8
                Return CInt(CurrentRank) >= 23
            Case 14
                Return Enumerable.Range(5, 9).Count(AddressOf ThemeCheckOne) >= 5
            Case Else
                If Settings.Get(Of String)("UiLauncherThemeHide").Contains("7") Then ThemeUnlock(7, ShowDoubleHint:=False)
                Return Settings.Get(Of String)("UiLauncherThemeHide2").ToString.Split("|", False).Contains(Id.ToString)
        End Select
    End Function
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        If FrmMain Is Nothing Then Return False
        If IsSponsoredFeaturesUnlocked Then
            If ShowDoubleHint Then Hint($"隐藏主题 {Id} 已被解锁！", HintType.Green)
            Return False
        End If
        If Id = 8 Then
            Throw New Exception("不可使用此方式解锁秋仪金！")
        End If
        Dim List As New List(Of String)(Settings.Get(Of String)("UiLauncherThemeHide2").ToString.Split("|", False))
        If List.Contains(Id.ToString) Then
            If ShowDoubleHint Then Hint("该隐藏主题已被解锁！")
            Return False
        End If
        Logger.Info($"解锁隐藏主题：{Id}")
        If UnlockHint IsNot Nothing Then Hint(UnlockHint, HintType.Green, Log:=False)
        List.Add(Id.ToString)
        Settings.Set("UiLauncherThemeHide2", String.Join("|", List))
        ThemeCheckAll(True)
        Return True
    End Function

#End Region

#Region "更新"

    Friend Sub UpdateCheckByButton()
        Hint("该版本中不包含更新功能……")
    End Sub

    Friend IsUpdateWaitingRestart As Boolean = False
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
    End Sub
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
    End Sub

    ''' <summary>
    ''' 确保 PathTemp/Latest.exe 是最新正式版的 PCL，它会被用于整合包打包。
    ''' 如果不是，则下载一个。
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        '注意：如果要自行实现这个功能，请换用另一个文件路径，以免与官方版本冲突
    End Sub

#End Region

#Region "联网配置"

    ''' <summary>
    ''' 联网获取的配置信息。
    ''' 若获取失败或仍在获取中，可能为 Nothing。
    ''' </summary>
    Public ServerConfig As JObject

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL 配置更新", Sub() Logger.Info("该版本中不包含更新通知功能……"), Priority:=ThreadPriority.BelowNormal) With
        {.ReloadTimeout = 1000 * 60 * 60} '超时 1 小时

#End Region

#Region "赞助等级"

    Private _PotatoFeatures As List(Of String) = Nothing
    Private Const EcdsaPublicKey As String = "RUNTMSAAAAC4QTUNAewh23Q4Q6koHkyIrDIIZUSbua23sf2DiZmIRwSzadISDRyTVTbuWniH3KR7rKj8XBsabms1be6i3c+S"

    ''' <summary>
    ''' 当前赞助等级，取土豆码特性列表中最高的等级。
    ''' </summary>
    Public ReadOnly Property CurrentRank As DonationRank
        Get
            If IsSponsoredFeaturesUnlocked Then Return DonationRank.Rank98
            For Each Rank In {DonationRank.Rank98, DonationRank.Rank54, DonationRank.Rank23, DonationRank.Rank12, DonationRank.Rank6}
                If PotatoFeatures.Contains(Rank.ToString) Then Return Rank
            Next
            Return DonationRank.None
        End Get
    End Property

    Private ReadOnly Property HasUpdatePermission As Boolean
        Get
            If IsSponsoredFeaturesUnlocked Then Return True
            If CInt(CurrentRank) < 12 Then
                Return PotatoFeatures.Contains("Update")
            End If
            Return True
        End Get
    End Property

    ''' <summary>
    ''' 校验全部土豆码后得到的特性列表。
    ''' </summary>
    Private ReadOnly Property PotatoFeatures As List(Of String)
        Get
            If IsSponsoredFeaturesUnlocked Then
                Return New List(Of String)({"Theme5", "Theme6", "Theme7", "Theme8", "Theme9", "Theme10", "Theme11", "Theme12", "Theme13", "Theme14", "Update"})
            End If
            If _PotatoFeatures Is Nothing Then
                _PotatoFeatures = New List(Of String)
                For Each Code In Settings.Get(Of String)("Potatoes").ToString.Split("|", True)
                    Dim Features = CheckPotatoCode(Code)
                    If TypeOf Features Is List(Of String) Then
                        _PotatoFeatures.AddRange(CType(Features, List(Of String)))
                    End If
                Next
                _PotatoFeatures = _PotatoFeatures.Distinct.ToList
            End If
            Return _PotatoFeatures
        End Get
    End Property

    ''' <summary>
    ''' 校验一个土豆码。成功时返回其包含的特性列表，失败时返回错误提示文本。
    ''' </summary>
    Private Function CheckPotatoCode(Key As String) As Object
        If String.IsNullOrEmpty(Key) Then
            Return "输入的土豆码为空。"
        ElseIf Key.Length < 30 AndAlso Key.ContainsIgnoreCase(Identify) Then
            Return $"你输入的是识别码，不是土豆码。{vbCrLf}要获取土豆码，请在爱发电私信发送【土豆 {Identify}】。"
        ElseIf Key.Length > 85 AndAlso Key.Length < 90 AndAlso Key.EndsWithF("=") Then
            Return $"你输入的是解锁码，不是土豆码。{vbCrLf}要获取土豆码，请在爱发电私信发送【土豆】。"
        ElseIf Key.Length > 150 AndAlso Key.Length < 165 AndAlso Key.EndsWithF("11") Then
            Return $"你输入的是更新密钥，不是土豆码。{vbCrLf}要获取土豆码，请在爱发电私信发送【土豆】。"
        ElseIf Not Key.StartsWithF("CD", True) OrElse Not Key.Contains("#") Then
            Return $"你输入的不是土豆码……有一说一，这是啥玩意儿？{vbCrLf}要获取土豆码，请在爱发电私信发送【土豆 {Identify}】。"
        ElseIf Key(2) > "1"c Then
            Return $"这个土豆码得在新版 PCL 上才能使用。{vbCrLf}请更新 PCL，或是在爱发电私信发送【下载】，然后在新下载的 PCL 上输入土豆码。"
        ElseIf "1"c > Key(3) Then
            Return $"这个土豆码不太新鲜，已经过期了。{vbCrLf}请在爱发电私信重新发送【土豆 {Identify}】，获取更新鲜的土豆码。"
        ElseIf Not Key.StartsWithF("CD") Then
            Return $"土豆码区分大小写，但你把一些大写的东西输成小写的了。{vbCrLf}你可以选中文本，然后右键复制粘贴，不必手动输入。"
        Else
            Try
                Dim Parts = Key.Substring(4).Replace("p", "=").Replace("%", "p").Split("#"c)
                Dim Content = Encoding.UTF8.GetString(Convert.FromBase64String(Parts(1)))
                CryptographyUtils.EcdsaVerify(Identify & Key(2) & Key(3) & Content, Parts(0), EcdsaPublicKey)
                Return Content.Split(","c, True).ToList
            Catch ex As Exception
                Logger.Warn(ex, $"土豆码校验失败（{Key}）")
                Return $"土豆码有误，请检查：{vbCrLf}- 私信显示的识别码应当为 {Identify}，如果不是，请在私信发送【土豆 {Identify}】{vbCrLf}- 你输入的以下土豆码是否与私信中的一致：{vbCrLf}{Key}{vbCrLf}错误原因：{ex.Message}"
            End Try
        End If
    End Function

    ''' <summary>
    ''' 弹窗要求输入土豆码并进行校验。
    ''' </summary>
    Public Sub InputPotatoCode(IsUpdating As Boolean)
        Dim Text As String
        If IsUpdating Then
            If MyMsgBox($"请点击【复制命令】，然后将复制的命令粘贴到爱发电私信，获取土豆码。{vbCrLf}输入土豆码后，就能继续更新啦！", "验证爱发电赞助",
                        "输入土豆码", "复制命令", "取消", HighLight:=True,
                        Button2Action:=Sub()
                                          ClipboardSet("土豆 " & Identify, False)
                                          OpenWebsite("https://meloong.com/afd/message/df2dfad2960911e89c5252540025c377?potato")
                                      End Sub) = 3 Then Return
            Text = MyMsgBoxInput("输入土豆码", $"请在【爱发电】向龙腾猫跃发送【土豆 {Identify}】，然后输入自动回复的土豆码以继续更新。{vbCrLf}自动回复可能比较慢，发送后等一会儿刷新一下网页应该已经有了。", Nothing, New ObjectModel.Collection(Of Validate) From {New ValidateNullOrWhiteSpace})
        Else
            Text = MyMsgBoxInput("输入土豆码", $"请输入你获得的土豆码。{vbCrLf}你的识别码：{Identify}", Nothing, New ObjectModel.Collection(Of Validate) From {New ValidateNullOrWhiteSpace})
        End If
        If String.IsNullOrWhiteSpace(Text) Then Return
        Text = Text.ReplaceLineEndings("", False).Between("：", "在", False).Trim(" "c, "【"c, "】"c)
        If String.IsNullOrWhiteSpace(Text) Then Return
        Logger.Info($"输入的土豆码：{Text}")
        If Not IsUpdating AndAlso (Text = "PuzzleUnlock" & Identify.Substring(Identify.Length - 4) OrElse
                                    Text = "PuzzleUnlock" & Identify.Substring(Identify.Length - 5) OrElse
                                    Text = "PuzzleUnlock" & Identify.Substring(Identify.Length - 6)) Then
            MyMsgBox($"隐藏主题 极客蓝 已解锁！{vbCrLf}难道，这就是终点了？还是说……？", "解锁新隐藏主题！")
            ThemeUnlock(11)
            Return
        End If
        Try
            Dim Result = CheckPotatoCode(Text)
            Dim Features As List(Of String)
            If TypeOf Result Is String Then Throw New Exception(CStr(Result)) Else Features = CType(Result, List(Of String))
            Logger.Info($"土豆码包含的特性：{String.Join(", ", Features)}")
            If Features.Any(Function(f) Not PotatoFeatures.Contains(f)) Then
                Dim Codes = Settings.Get(Of String)("Potatoes").ToString.Split("|", True).ToList
                Codes.Add(Text)
                Settings.Set("Potatoes", String.Join("|", Codes))
                _PotatoFeatures = Nothing
            End If
            If IsUpdating Then
                If Not HasUpdatePermission Then Throw New Exception("这个土豆码里不包含更新权限……")
            Else
                Hint("已成功解锁！", HintType.Green)
            End If
        Catch ex As Exception
            Logger.Info($"土豆码无效：{ex.Message}")
            If MyMsgBox(ex.Message, "错误", "重新输入", "取消", IsWarn:=True) = 1 Then
                InputPotatoCode(IsUpdating)
            End If
        End Try
    End Sub
    Friend Sub GeneratePotatoCode()
    End Sub

    ''' <summary>
    ''' 获取设备识别码。
    ''' </summary>
    Friend Function GetIdentify() As String
        Try
            If ApplicationStartTick < 1 Then Return "0000-0000-0000-0000"
            Dim HardwareConfig As String
            Try
                HardwareConfig = RegistryUtils.TryRead("HKEY_LOCAL_MACHINE\SYSTEM\HardwareConfig", "LastConfig", "Unknown").ToString.Upper.Trim("{"c, "}"c)
            Catch ex As Exception
                Logger.Warn(ex, "获取主板标识码失败")
                HardwareConfig = "Unknown"
            End Try
            Dim Identify As String = Settings.Get(Of String)("Identify")
            If Identify.Length < 3 Then
                Identify = GetTimeMs & My.Computer.Info.AvailablePhysicalMemory
                Settings.Set("Identify", Identify)
            End If
            Dim Hash As String = $"{HardwareConfig}{Identify}".GetStableHashCode.ToString("X").EnsureLength("7"c, 16)
            Return $"{Hash.Substring(4, 4)}-{Hash.Substring(12, 4)}-{Hash.Substring(0, 4)}-{Hash.Substring(8, 4)}"
        Catch ex As Exception
            Logger.Error(ex, $"PCL 无法获取设备识别码，这可能会导致部分设置无法正常存储。{vbCrLf}{vbCrLf}详细的错误信息", LogBehavior.AlertThenFeedback)
            Return "0000-0000-0000-0000"
        End Try
    End Function

#End Region

End Module
