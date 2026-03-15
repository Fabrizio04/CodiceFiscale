using System.Text;
using System.Text.RegularExpressions;

namespace CodiceFiscale.Services
{
    public static class CodiceFiscaleService
    {
        public static string Calcola(string nome, string cognome, DateTime data, bool isMaschio, string codiceComune)
        {
            StringBuilder cf = new StringBuilder();

            // 1. COGNOME
            cf.Append(EstraiLettere(cognome, false));

            // 2. NOME
            cf.Append(EstraiLettere(nome, true));

            // 3. ANNO
            cf.Append(data.ToString("yy"));

            // 4. MESE
            string mesi = "ABCDEHLMPRST";
            cf.Append(mesi[data.Month - 1]);

            // 5. GIORNO E SESSO
            int giorno = data.Day;
            if (!isMaschio) giorno += 40;
            cf.Append(giorno.ToString("D2"));

            // 6. COMUNE
            cf.Append(codiceComune.ToUpper());

            // 7. CARATTERE DI CONTROLLO
            cf.Append(CalcolaCheckDigit(cf.ToString()));

            return cf.ToString().ToUpper();
        }

        private static string EstraiLettere(string testo, bool isNome)
        {
            testo = Regex.Replace(testo.ToUpper(), @"[^A-Z]", "");
            var consonanti = Regex.Replace(testo, @"[AEIOU]", "");
            var vocali = Regex.Replace(testo, @"[^AEIOU]", "");

            string risultato = "";

            if (isNome && consonanti.Length >= 4)
            {
                // Regola speciale per il nome: se ha >= 4 consonanti, si prendono 1^, 3^ e 4^
                risultato = $"{consonanti[0]}{consonanti[2]}{consonanti[3]}";
            }
            else
            {
                // Altrimenti: consonanti + vocali + X
                string unione = consonanti + vocali + "XXX";
                risultato = unione.Substring(0, 3);
            }

            return risultato;
        }

        private static char CalcolaCheckDigit(string cfParziale)
        {
            // Tabelle ufficiali Agenzia delle Entrate
            var dispari = new Dictionary<char, int> {
                {'0', 1}, {'1', 0}, {'2', 5}, {'3', 7}, {'4', 9}, {'5', 13}, {'6', 15}, {'7', 17}, {'8', 19}, {'9', 21},
                {'A', 1}, {'B', 0}, {'C', 5}, {'D', 7}, {'E', 9}, {'F', 13}, {'G', 15}, {'H', 17}, {'I', 19}, {'J', 21},
                {'K', 2}, {'L', 4}, {'M', 18}, {'N', 20}, {'O', 11}, {'P', 3}, {'Q', 6}, {'R', 8}, {'S', 12}, {'T', 14},
                {'U', 16}, {'V', 10}, {'W', 22}, {'X', 25}, {'Y', 24}, {'Z', 23}
            };

            var pari = new Dictionary<char, int> {
                {'0', 0}, {'1', 1}, {'2', 2}, {'3', 3}, {'4', 4}, {'5', 5}, {'6', 6}, {'7', 7}, {'8', 8}, {'9', 9},
                {'A', 0}, {'B', 1}, {'C', 2}, {'D', 3}, {'E', 4}, {'F', 5}, {'G', 6}, {'H', 7}, {'I', 8}, {'J', 9},
                {'K', 10}, {'L', 11}, {'M', 12}, {'N', 13}, {'O', 14}, {'P', 15}, {'Q', 16}, {'R', 17}, {'S', 18}, {'T', 19},
                {'U', 20}, {'V', 21}, {'W', 22}, {'X', 23}, {'Y', 24}, {'Z', 25}
            };

            int somma = 0;
            for (int i = 0; i < cfParziale.Length; i++)
            {
                char c = char.ToUpper(cfParziale[i]);

                // ATTENZIONE: i+1 serve perché l'algoritmo ufficiale conta da 1.
                // Posizione 1, 3, 5... (dispari)
                // Posizione 2, 4, 6... (pari)
                if ((i + 1) % 2 != 0)
                    somma += dispari[c];
                else
                    somma += pari[c];
            }

            int resto = somma % 26;
            return (char)('A' + resto);
        }
    }
}
