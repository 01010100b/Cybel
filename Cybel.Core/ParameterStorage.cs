using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public static class ParameterStorage
    {
        private static string Folder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "params");

        public static T Load<T>(IGame game) where T : new()
        {
            var file = Path.Combine(Folder, GetId<T>(game) + ".json");

            if (File.Exists(file))
            {
                var t = JsonSerializer.Deserialize<T>(File.ReadAllText(file));
                
                if (t is not null)
                {
                    return t;
                }
            }

            return new T();
        }

        public static void Save<T>(IGame game, T parameters)
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            var options = new JsonSerializerOptions() { WriteIndented = true };
            var json = JsonSerializer.Serialize(parameters, options);
            var file = Path.Combine(Folder, GetId<T>(game) + ".json");

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            File.WriteAllText(file, json);
        }

        private static string GetId<T>(IGame game)
        {
            return $"p {typeof(T).Name} {game.Id}";
        }
    }
}
