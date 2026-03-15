using CodiceFiscale.Models;
using CodiceFiscale.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CodiceFiscale.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    public MainViewModel()
    {
        _dbService = new DatabaseService();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalcolaCommand))]
    string cognome;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalcolaCommand))]
    string nome;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalcolaCommand))]
    DateTime dataNascita = DateTime.Now.AddYears(-30); // Default a 30 anni fa

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalcolaCommand))]
    bool isMaschio = true; // Default Maschio

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalcolaCommand))]
    bool isFemmina;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalcolaCommand))]
    string comuneDigitato;

    [ObservableProperty]
    string risultatoCF = "IL TUO CODICE FISCALE";

    // Aggiungi questo ai tuoi ObservableProperty
    [ObservableProperty]
    private List<Comune> suggerimentiComuni;

    [ObservableProperty]
    private bool isSuggerimentiVisible;

    private bool _isSelecting;

    partial void OnCognomeChanged(string value)
    {
        if (value != null)
        {
            // Forziamo il maiuscolo senza creare loop infiniti
            var upper = value.ToUpper();
            if (value != upper) Cognome = upper;
        }
    }

    partial void OnNomeChanged(string value)
    {
        if (value != null)
        {
            var upper = value.ToUpper();
            if (value != upper) Nome = upper;
        }
    }

    // Logica per i suggerimenti (da chiamare quando cambia ComuneDigitato)
    // Puoi usare il metodo OnComuneDigitatoChanged se usi CommunityToolkit
    // Modifica la logica di cambio testo
    partial void OnComuneDigitatoChanged(string value)
    {
        // Se stiamo selezionando dalla lista, usciamo subito senza mostrare nulla
        if (_isSelecting) return;

        if (value != null)
        {
            var upper = value.ToUpper();
            if (value != upper) ComuneDigitato = upper;
        }

        if (!string.IsNullOrWhiteSpace(value) && value.Length >= 3)
        {
            AggiornaSuggerimenti(value);
        }
        else
        {
            IsSuggerimentiVisible = false;
        }
    }

    private async void AggiornaSuggerimenti(string filtro)
    {
        var comuni = await _dbService.CercaComuniAsync(filtro);
        SuggerimentiComuni = comuni;
        IsSuggerimentiVisible = comuni.Any();
    }

    // Modifica il comando di selezione
    [RelayCommand]
    private void SelezionaComune(Comune comune)
    {
        if (comune != null)
        {
            _isSelecting = true; // Attiviamo il flag

            ComuneDigitato = comune.Nome;
            IsSuggerimentiVisible = false;

            _isSelecting = false; // Lo resettiamo subito dopo
        }
    }



    // Logica di validazione per il tasto Calcola
    private bool CanCalcola()
    {
        return !string.IsNullOrWhiteSpace(Nome) &&
               Nome.Length >= 2 &&
               !string.IsNullOrWhiteSpace(Cognome) &&
               Cognome.Length >= 2 &&
               !string.IsNullOrWhiteSpace(ComuneDigitato) &&
               DataNascita < DateTime.Now;
    }

    [RelayCommand(CanExecute = nameof(CanCalcola))]
    private async Task Calcola()
    {
        // Recuperiamo il codice del comune dal DB prima del calcolo
        var comuni = await _dbService.CercaComuniAsync(ComuneDigitato);
        var comuneValido = comuni.FirstOrDefault(c => c.Nome.Equals(ComuneDigitato, StringComparison.OrdinalIgnoreCase));

        if (comuneValido != null)
        {
            RisultatoCF = CodiceFiscaleService.Calcola(
                Nome,
                Cognome,
                DataNascita,
                IsMaschio,
                comuneValido.CodiceCatastale);
        }
        else
        {
            await Application.Current.Windows[0].Page.DisplayAlertAsync("Errore", "Il comune inserito non è valido.", "OK");
        }
    }

    // Comando per Copiare negli Appunti
    [RelayCommand]
    private async Task CopiaCodice()
    {
        if (RisultatoCF != "IL TUO CODICE FISCALE" && !string.IsNullOrWhiteSpace(RisultatoCF))
        {
            await Clipboard.Default.SetTextAsync(RisultatoCF);

            // NOTA: A partire da Android 13, il sistema operativo mostra un messaggio Toast a prescindere

#if WINDOWS
            //await Application.Current.Windows[0].Page.DisplayAlertAsync("Copiato", "Codice Fiscale copiato negli appunti!", "OK");
            var messaggio = "Codice Fiscale copiato negli appunti!";
            var snackbar = Snackbar.Make(messaggio);

            await snackbar.Show();
#endif
        }
    }

    // Comando per resettare il Form
    [RelayCommand]
    private void ResetForm()
    {
        _isSelecting = false; // Reset del flag
        Cognome = string.Empty;
        Nome = string.Empty;
        DataNascita = DateTime.Now.AddYears(-30);
        IsMaschio = true;
        ComuneDigitato = string.Empty;
        RisultatoCF = "IL TUO CODICE FISCALE";
        SuggerimentiComuni = null;
        IsSuggerimentiVisible = false;
    }

    [RelayCommand]
    private void ChiudiSuggerimenti()
    {
        IsSuggerimentiVisible = false;
    }
}