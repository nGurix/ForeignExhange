using ForeignExhange.Models;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Input;
using Xamarin.Forms;

namespace ForeignExhange.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        #region Attributes
        bool isRunning;
        bool isEnable;
        string result;
        ObservableCollection<Rate> rates;
        Rate sourceRate;
        Rate targetRate;
        #endregion

        #region Properties
        public string Amount { get; set; }
        public ObservableCollection<Rate> Rates
        {
            get
            {
                return rates;
            }
            set
            {
                if (rates != value)
                {
                    rates = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rates)));
                }
            }
        }
        public Rate SourceRate {
            get
            {
                return sourceRate;
            }
            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceRate)));
                }
            }
        }
        public Rate TargetRate
        {
            get
            {
                return targetRate;
            }
            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetRate)));
                }
            }
        }
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }
        }
        public bool IsEnable
        {
            get
            {
                return isEnable;
            }
            set
            {
                if (isEnable != value)
                {
                    isEnable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnable)));
                }
            }
        }
        public string Result
        {
            get
            {
                return result;
            }
            set
            {
                if (result != value)
                {
                    result = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Result)));
                }
            }
        }
        #endregion


        #region Constructors
        public MainViewModel()
        {
            LoadRates();
        }
        #endregion

        #region Methods
        async void LoadRates()
        {
            IsRunning = true;
            Result = "Loading rates...";
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://desaapiexchangerates.azurewebsites.net");
                var controller = "/api/Rates";
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    IsRunning = false;
                    Result = result;
                }

                var rates = JsonConvert.DeserializeObject<List<Rate>>(result);
                Rates = new ObservableCollection<Rate>(rates);

                IsRunning = false;
                IsEnable = true;
                result = "ready to convert...";
            }
            catch (Exception ex)
            {

                IsRunning = false;
                Result = ex.Message;
            }
        }
        #endregion

        #region Commands
        public ICommand ChangeCommand
        {
            get
            {
                return new RelayCommand(Change);
            }
        }

        private void Change()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            Convert();
        }

        public ICommand ConvertCommand
        {
            get
            {
                return new RelayCommand(Convert);
            }
        }

        async void Convert()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "You must enter a value in amount.", "Accept");
                return;
            }

            decimal amount = 0;
            if(!decimal.TryParse(Amount, out amount))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "You must enter a numeric value in amount.", "Accept");
                return;
            }

            if(SourceRate == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "You must select a source rate.", "Accept");
                return;
            }

            if (TargetRate == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "You must select a target rate.", "Accept");
                return;
            }

            var amountConverted = amount / (decimal)SourceRate.TaxRate * (decimal)TargetRate.TaxRate;

            Result = string.Format("{0} {1:C2} = {2} {3:C2}", SourceRate.Code, amount, targetRate.Code, amountConverted);
        }
        #endregion
    }
}
