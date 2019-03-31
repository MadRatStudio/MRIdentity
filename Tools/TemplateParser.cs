using Infrastructure.System.Appsettings;
using RazorLight;
using System.IO;
using System.Threading.Tasks;

namespace Tools
{
    public class TemplateParser
    {
        protected readonly TemplateSettings _settings;
        protected readonly RazorLightEngine _engine;

        public TemplateParser(TemplateSettings settings)
        {
            _settings = settings;

            _engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(Path.Combine(Directory.GetCurrentDirectory(), _settings.RelativePath))
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> Render<T>(string folder, string file, T model)
        {
            var path = string.IsNullOrWhiteSpace(folder) ? file : Path.Combine(folder, file);
            if (!path.EndsWith(".cshtml") && !path.EndsWith(".html"))
                path += ".cshtml";

            return await _engine.CompileRenderAsync<T>(path, model);
        }

        public async Task<string> Render<T>(string file, T model)
        {
            if (!file.EndsWith(".cshtml") && !file.EndsWith(".html"))
                file += ".cshtml";

            return await _engine.CompileRenderAsync<T>(file, model);
        }
    }
}
