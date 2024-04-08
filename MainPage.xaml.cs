using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SystemLosowania_MartaBiala
{
    public partial class MainPage : ContentPage
    {
        private List<string> listaUczniow = new List<string>();
        private string plikListaUczniow = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ListaUczniow.txt");
        private Dictionary<string, List<string>> klasy = new Dictionary<string, List<string>>();
        private string aktualnaKlasa = "";

        public MainPage()
        {
            InitializeComponent();
            WczytajListeUczniow();
            OdswiezListeKlas();
        }

 


        private void WczytajListeUczniow()
        {
            if (File.Exists(plikListaUczniow))
            {
                var json = File.ReadAllText(plikListaUczniow);
                listaUczniow = JsonSerializer.Deserialize<List<string>>(json);
                ListaUczniow.ItemsSource = listaUczniow;
            }
            else
            {
                listaUczniow = new List<string>();
            }
        }

        private void ZapiszListeUczniow()
        {
            var json = JsonSerializer.Serialize(listaUczniow);
            File.WriteAllText(plikListaUczniow, json);
        }

        public void ZapiszListeUczniowZewnetrznie()
        {
            ZapiszListeUczniow();
        }

        private async void WczytajListeZPliku_Clicked(object sender, EventArgs e)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Wybierz plik",
                };

                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    var fileStream = await result.OpenReadAsync();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        var fileContent = await reader.ReadToEndAsync();
                        listaUczniow = JsonSerializer.Deserialize<List<string>>(fileContent);
                        ListaUczniow.ItemsSource = listaUczniow;
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędów
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
            }
        }

        private void DodajDoKlasy_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ImieUcznia.Text) && !string.IsNullOrEmpty(aktualnaKlasa))
            {
                if (klasy.ContainsKey(aktualnaKlasa))
                {
                    klasy[aktualnaKlasa].Add(ImieUcznia.Text);
                    OdswiezListeKlas();
                }
                else
                {
                    // Dodaj do ogólnej listy uczniów, jeśli klasa nie istnieje
                    listaUczniow.Add(ImieUcznia.Text);
                    ZapiszListeUczniow();
                }

                ImieUcznia.Text = string.Empty;
            }
        }

        private void OdswiezListeKlas()
        {
            StackKlasy.Children.Clear();
            foreach (var klasa in klasy)
            {
                StackLayout stackLayoutKlasa = new StackLayout();
                Label labelKlasa = new Label
                {
                    Text = $"Klasa: {klasa.Key}",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    GestureRecognizers = { new TapGestureRecognizer { Command = new Command(() => KlasaLabelClicked(klasa.Key)) } }
                };

                // Podświetl na fioletowo, jeśli jest to aktualnie wybrana klasa
                if (klasa.Key == aktualnaKlasa)
                {
                    labelKlasa.TextColor = Color.FromHex("#800080");
                }

                stackLayoutKlasa.Children.Add(labelKlasa);

                for (int i = 0; i < klasa.Value.Count; i++)
                {
                    Label labelUczen = new Label { Text = $"- {i + 1}. {klasa.Value[i]}" };
                    stackLayoutKlasa.Children.Add(labelUczen);
                }

                StackKlasy.Children.Add(stackLayoutKlasa);
            }
        }


        private void StworzNowaKlase_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NazwaNowejKlasy.Text))
            {
                string nowaKlasa = NazwaNowejKlasy.Text;
                if (!klasy.ContainsKey(nowaKlasa))
                {
                    klasy.Add(nowaKlasa, new List<string>());
                    DisplayAlert("Sukces", $"Utworzono nową klasę: {nowaKlasa}", "OK");

                    // Wybierz nową klasę po utworzeniu
                    KlasaLabelClicked(nowaKlasa);
                }
                else
                {
                    DisplayAlert("Błąd", $"Klasa o nazwie {nowaKlasa} już istnieje.", "OK");
                }

                NazwaNowejKlasy.Text = string.Empty;
                OdswiezListeKlas();
            }
        }

        private void KlasaLabelClicked(string nazwaKlasy)
        {
            aktualnaKlasa = nazwaKlasy;
            // Odswieżenie listy klas
            OdswiezListeKlas();

            // Przekierowanie na nową stronę z listą uczniów danej klasy
            Navigation.PushAsync(new EdycjaKlasyPage(nazwaKlasy, klasy[nazwaKlasy]));
        }

        private void LosujUcznia_Clicked(object sender, EventArgs e)
        {
            if (listaUczniow.Count > 0)
            {
                Random random = new Random();
                int index = random.Next(0, listaUczniow.Count);
                string losowanyUczen = listaUczniow[index];
                DisplayAlert("Losowanie Ucznia", $"Wylosowany uczeń: {losowanyUczen}", "OK");
            }
            else
            {
                DisplayAlert("Brak uczniów", "Lista uczniów jest pusta.", "OK");
            }
        }
    }
}
