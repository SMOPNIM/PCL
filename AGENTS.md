##  Git 规范 (Git Workflow)

本项目采用 **Fork + 子模块 (Submodules)** 架构，请严格遵守以下 Git 操作规范，以防止提交丢失或破坏仓库结构。

### 1. 提交信息规范 (Conventional Commits)
所有提交必须遵循约定式提交格式：`<type>(<scope>): <subject>`。

**强制要求：所有的 subject（标题）和 body（正文）必须使用中文编写。**

*   **允许的 Type**：
    *   `feat`: 新功能
    *   `fix`: 修复 Bug
    *   `chore`: 杂项/配置修改（如：更新 `.gitmodules`、修改 `.gitignore`）
    *   `build`: 构建系统或外部依赖变动（如：移除构建产物 `.zip`）
    *   `docs`: 文档修改
    *   `refactor`: 代码重构
*   **中文示例**：
    *   `feat(ui): 增加暗黑模式切换按钮`
    *   `chore(submodule): 将核心模块 URL 指向个人 fork`
    *   `build: 移除根目录下的构建产物 最新正式版.zip`
    *   `fix(auth): 修复登录时 Token 过期的报错`

### 2. 分支管理 (Branching)
*   **主分支**：默认分支为 `dev` ，禁止直接向主分支强推 (`git push -f`)。
*   **新建分支**：从最新的 `dev` 切出，命名格式建议为 `feat/xxx` 或 `fix/xxx`。
*   **多仓库分支同步**：本项目包含子模块。**新建分支时，必须确保父仓库和所有子模块（Submodules）都切换到了同名的本地分支。**
    *   *注意*：如果在 IDE (如 IntelliJ/PyCharm) 中新建分支，请务必选择**“所有仓库 (All Repositories)”**，以防子模块处于分离头指针 (Detached HEAD) 状态导致提交丢失。

### 3. 子模块操作规范 (Submodules)
*   **克隆**：必须使用 `git clone --recurse-submodules <url>`，或克隆后执行 `git submodule update --init --recursive`。
*   **修改子模块**：
    1.  **前置条件**：如果子模块指向的是官方仓库，在修改前必须先将该子模块 Fork 到个人账号，并修改本地子模块的 `origin` 为你的 Fork 地址。
    2.  **提交顺序**：必须在子模块目录内先 `commit` 并 `push` 到远程。
    3.  **更新父仓库**：子模块推送成功后，回到父仓库根目录，`git add <子模块路径>` 更新指针 (Gitlink)，然后再进行父仓库的提交。
*   **同步配置**：若修改了根目录的 `.gitmodules`，必须执行 `git submodule sync --recursive` 以同步本地配置。

### 4. 文件清理与忽略 (.gitignore)
*   **禁止提交二进制文件**：绝对不要将构建产物（如 `.zip`, `.exe`, `.dll`）、临时文件或大型数据集提交到 Git 仓库。
*   **处理误提交**：若不慎提交了构建产物，请使用 `git rm <filename>` 将其从版本控制中移除，并立即在 `.gitignore` 中添加对应的忽略规则（如 `*.zip`）。

### 5. 远程同步 (Upstream & Origin)
*   `origin`：指向个人的 Fork 仓库，用于日常推送。
*   `upstream`：指向官方原仓库（本项目的 `upstream` push URL 已被设置为 `No_Push` 以防误推）。
*   **同步官方更新**：
    ```bash
    git fetch upstream
    git switch main
    git merge upstream/main
    git submodule update --recursive
    ```
    