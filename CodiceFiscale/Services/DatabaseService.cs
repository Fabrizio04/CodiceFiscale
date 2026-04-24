using CodiceFiscale.Models;
using SQLite;

namespace CodiceFiscale.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        public const SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLiteOpenFlags.SharedCache;

        private const string DB_NAME = "CodiceFiscale.db3";

        private async Task Init()
        {
            if (_database is not null)
                return;

            //var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "CodiceFiscale.db3");
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.PackageName);
            
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);

            dbPath = Path.Combine(dbPath, DB_NAME);

            //Cancellazione
            //if (File.Exists(dbPath))
            //{
            //    File.Delete(dbPath);
            //}

            _database = new SQLiteAsyncConnection(dbPath, Flags);

            // Crea la tabella se non esiste
            await _database.CreateTableAsync<Comune>();

            // Controlla se è vuota. Se sì, popolala.
            var count = await _database.Table<Comune>().CountAsync();
            if (count == 0)
            {
                await PopolaDatabaseIniziale();
            }
        }

        private async Task PopolaDatabaseIniziale()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("comuni.csv");
            using var reader = new StreamReader(stream);

            var comuni = new List<Comune>();
            string line;

            // Leggiamo riga per riga finché ReadLineAsync non restituisce null
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');

                // Dato che hai tolto l'intestazione e le colonne extra, 
                // verifica bene gli indici. Se hai solo Nome e Codice, saranno [0] e [1].
                comuni.Add(new Comune
                {
                    Nome = values[1].Trim().ToUpper(),
                    CodiceCatastale = values[0].Trim().ToUpper()
                });
            }

            if (comuni.Count != 0)
            {
                await _database.InsertAllAsync(comuni);
            }
        }

        public async Task<List<Comune>> CercaComuniAsync(string filtro)
        {
            await Init();
            return await _database.Table<Comune>()
                                  .Where(c => c.Nome.Contains(filtro.ToUpper()))
                                  .ToListAsync();
        }
    }
}
