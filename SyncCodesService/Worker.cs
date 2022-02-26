namespace SyncCodesService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _workSpace;
        private FileSystemWatcher _watcher;

        private bool _refreshed = false;
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
            if(string.IsNullOrEmpty(Args.WorkPlace))
            {
                _workSpace = config["MyConfig:WorkSpace"];
            }
            else
            {
                _workSpace = Args.WorkPlace;
            }

            string codesRoot = Path.Combine(_workSpace, "Codes");
            _watcher = new FileSystemWatcher(codesRoot);
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;

            _logger.LogInformation($"正在监听目录{new DirectoryInfo(codesRoot).FullName}，按ESC可退出。");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                }
                _refreshed = false;
                await Task.Delay(1000, stoppingToken);
            }
            _watcher.EnableRaisingEvents = false;
            Environment.Exit(0);
        }


        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Refresh(e.FullPath);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Refresh(e.FullPath);
        }

        private void Refresh(string path)
        {
            if (_refreshed)
            {
                return;
            }

            if (Path.GetExtension(path).ToLower() != ".cs")
            {
                return;
            }
            string root = _workSpace;
            if (!Directory.Exists(root))
            {
                _logger.LogError($"目录{root}不存在,检查参数");
            }
            AdjustTool.Adjust(root + @"\Unity.Model.csproj", "Model");
            AdjustTool.Adjust(root + @"\Unity.ModelView.csproj", "ModelView");
            AdjustTool.Adjust(root + @"\Unity.Hotfix.csproj", "Hotfix");
            AdjustTool.Adjust(root + @"\Unity.HotfixView.csproj", "HotfixView");
            _refreshed = true;
        }
    }
}