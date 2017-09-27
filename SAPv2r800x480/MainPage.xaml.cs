using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Media;
using System.Xml.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace SAP
{
    public sealed partial class MainPage : Page
    {
        // Dispatcher timer for the clock 
        private DispatcherTimer DispatcherClockTimer = new DispatcherTimer();
        private static bool twentyFourHrCbPreviousState = false;
        private static bool twelveHrCbPreviousState = true;
        private static bool broadcasted = false;

        // Dispatcher timer for aquaponics light switch 
        private DispatcherTimer DispatcherLightTimer = new DispatcherTimer();

        // Dispatcher timer for gauge update
        private DispatcherTimer DispatcherUpdateGaugeTimer = new DispatcherTimer();

        // Initializes a new instance of the XDocument class (represents an XML document)
        XDocument xdoc = new XDocument();

        // Instantiate a new instance of MediaElement
        // Represents an object that renders audio and video to the display
        MediaElement mediaElement = new MediaElement();

        // RPI GPIO Settings
        private const int LightSwitch_PIN = 23;        
        private GpioPin LightSwitchPin;        

        public MainPage()
        {
            this.InitializeComponent();             

            // Timer setup for the Digital Clock
            DispatcherClockTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherClockTimer.Tick += DispatcherClockTimer_Tick;
            DispatcherClockTimer.Start();

            // Timer setup for the Sensor Reading Update
            DispatcherUpdateGaugeTimer.Interval = TimeSpan.FromSeconds(60);
            DispatcherUpdateGaugeTimer.Tick += DispatcherUpdateGaugeTimer_Tick;
            UpdateGauges();
            DispatcherUpdateGaugeTimer.Start();

            // Timer setup for the Aquaponics Light
            DispatcherLightTimer.Interval = TimeSpan.FromSeconds(60);
            DispatcherLightTimer.Tick += DispatcherLightTimer_Tick;
            
            // 
            //Speak(
            //    "Hi, my name is Leo. I am your intelligent agent for Project Leo: Prototype Version 1.0." +
            //    "  " +
            //    "and the latest version of Smart Aquaponics System Prototype Version 2.0.");

            // Update Location TextBlock                       
            if (GetLocationByIPAddress() == null || GetLocationByIPAddress() == "")
            {
                this.SystemStatusTb.Text = "System Status: Error getting location by IP Address!";
                this.Lbl_Location.Text = "Singapore, Singapore";
                Speak(
                    "I am having problem getting your location." +
                    " " +
                    "Please check your network.");
            }
            else
                this.Lbl_Location.Text = GetLocationByIPAddress();

            // Initialize Status TextBlock
            this.SystemStatusTb.Text = "System Status: It's all good :)";
                        
            InitGPIO();

            if (LightSwitchPin != null)
                DispatcherLightTimer.Start();                   
        }        

        /// <summary>
        /// Speech Synthesizer
        /// Text to Speech method (Speak)
        /// </summary>
        private async void Speak(string text)
        {
            // Dispose SpeechRecognizer 
            //await DisposeSpeechRecognizer();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {                
                _Speak(text);                
            });            
        }

        /// <summary>
        /// Speech Synthesizer
        /// Text to Speech method (Speak)
        /// </summary>
        private async void _Speak(string text)
        {
            // Stops and resets media 
            mediaElement.Stop();

            // Initialize a new instance of the SpeechSynthesizer
            SpeechSynthesizer synth = new SpeechSynthesizer();

            // Generate the audio stream from plain text
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(text);
			
            // Send the stream to the media object
            mediaElement.SetSource(stream, stream.ContentType);                       

            // Disposes the SpeechSynthesizer object and releases resources used 
            synth.Dispose();
        }

        /// <summary>
        /// check and switch ON/OFF the light
        /// </summary>
        private void DispatcherLightTimer_Tick(object sender, object e)
        {
            // Grow light/s
            if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
            {
                // Lights need to be ON 

                if (LightSwitchPin.Read() == GpioPinValue.Low)     // check if the relay is switched OFF
                {
                    LightSwitchPin.Write(GpioPinValue.High);
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-on.png")), Stretch = Stretch.Fill };
                    Speak("Lights ON");
                    
                }
                else
                {
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-on.png")), Stretch = Stretch.Fill };
                }
            }
            else
            {
                if (LightSwitchPin.Read() == GpioPinValue.High)
                {
                    LightSwitchPin.Write(GpioPinValue.Low);
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-off.png")), Stretch = Stretch.Fill };
                    Speak("Lights OFF");
                    
                }
                else
                {
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-off.png")), Stretch = Stretch.Fill };
                }
            }            
        }

        /// <summary>
        /// Updates the current time and date display
        /// </summary>
        private void DispatcherClockTimer_Tick(object sender, object e)
        {
            TwentyFourHrCb.IsChecked = twentyFourHrCbPreviousState;
            TwelveHrCb.IsChecked = twelveHrCbPreviousState;
            
            this.Lbl_Time.Text = (TwelveHrCb.IsChecked == true) ? DateTime.Now.ToString("hh:mm tt") : DateTime.Now.ToString("HH:mm tt");
            this.Lbl_Date.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            if (DateTime.Now.Minute == 0 && broadcasted == false && DateTime.Now.Hour > 5)
            {
                if (DateTime.Now.Hour <= 12)
                    Speak("It's " + DateTime.Now.Hour + " o'clock!");
                else
                    Speak("It's " + (DateTime.Now.Hour - 12) + " o'clock!");
                broadcasted = true;
            }
            if (DateTime.Now.Minute == 1)           
                broadcasted = false; 
        }        

        /// <summary>
        /// Returns the private IP address of the device
        /// </summary>
        public IPAddress GetIPAddress()
        {
            List<string> IpAddress = new List<string>();
            var Hosts = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().ToList();
            foreach (var Host in Hosts)
            {
                string IP = Host.DisplayName;
                IpAddress.Add(IP);
            }
            IPAddress address = IPAddress.Parse(IpAddress.Last());
            return address;
        }

        /// <summary>
        /// Returns the public IP address of the device 
        /// </summary>
        public string GetPublicIPAddress()
        {
            string ip = String.Empty;
            try
            {
                string uri = "http://whatismyip.bitsflipper.com/";
                
                using (var client = new HttpClient())
                {
                    var result = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                    ip = result.Split(':')[1].Split('<')[0];
                }
            }
            catch(Exception)
            {
                this.SystemStatusTb.Text = "Exception: Get Public Address Error!";                
            }            
            return ip;            
        }

        /// <summary>
        /// Returns the location (city and country) of the device
        /// </summary>
        public string GetLocationByIPAddress()
        {
            string location = String.Empty;
            try
            {
                string uri = "http://whatismyip.bitsflipper.com/location.php";
                using (var client = new HttpClient())
                {
                    var result = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                    location = result.Split(':')[1].Split('<')[0];
                }
            }
            catch(Exception)
            {
                this.SystemStatusTb.Text = "Exception: Get Location By IP Address Error!";                
            }            
            return location.Trim();
        }     

        /// <summary>
        /// 12-hr clock time keeping convention
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TwelveHrCb_Checked(object sender, RoutedEventArgs e)
        {
            if(twelveHrCbPreviousState == false)
                Speak("Changing time keeping convention to 12-hour clock.");
            twelveHrCbPreviousState = true;
            twentyFourHrCbPreviousState = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Lbl_Time.Text = DateTime.Now.ToString("hh:mm tt"); // 12hr format
            });
            
        }

        /// <summary>
        /// 24-hr clock convention
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TwentyFourHrCb_Checked(object sender, RoutedEventArgs e)
        {
            if(twentyFourHrCbPreviousState == false)
                Speak("Changing time keeping convention to 24-hour clock.");
            twentyFourHrCbPreviousState = true;
            twelveHrCbPreviousState = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Lbl_Time.Text = DateTime.Now.ToString("HH:mm tt"); // 24hr format
            }); 
        }       
        

        /// <summary>
        /// Initialize RPI GPIO Pin/s
        /// </summary>
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Check GPIO controller
            if (gpio == null)
            {
                LightSwitchPin = null;                
                SystemStatusTb.Text = "System Status: There is no GPIO controller on this device";
                Speak("There is no GPIO controller on this device.");
                return;
            }

            LightSwitchPin = gpio.OpenPin(LightSwitch_PIN);
            LightSwitchPin.Write(GpioPinValue.Low);
            LightSwitchPin.SetDriveMode(GpioPinDriveMode.Output);
            
            if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
            {
                // Lights need to be ON 
                // LightSwitchPin = GpioPinValue.High
                if (LightSwitchPin.Read() == GpioPinValue.Low)     // check if the relay is switched ON
                {
                    LightSwitchPin.Write(GpioPinValue.High);
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-on.png")), Stretch = Stretch.Fill };
                    Speak("Lights ON");
                    
                }
                else
                {
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-on.png")), Stretch = Stretch.Fill };
                }
            }
            else
            {
                if (LightSwitchPin.Read() == GpioPinValue.High)
                {
                    LightSwitchPin.Write(GpioPinValue.Low);
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-off.png")), Stretch = Stretch.Fill };
                    Speak("Lights OFF");                    
                }
                else
                {
                    LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-off.png")), Stretch = Stretch.Fill };
                }
            }

            
        }

        /// <summary>
        /// Updates the gauges
        /// </summary>
        private async void DispatcherUpdateGaugeTimer_Tick(object sender, object e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateGauges());
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateGauges()
        {
            float wtemp;    // water temperature
            float atemp;    // ambient temperature
            float rh;       // relative humidity
            float fr;       // flow rate
            float ph;       // water pH value
            float do2;      // dissolved oxygen in the water
            string tstamp;  // last update

            try
            {
                string response = String.Empty;
                string url = "http://siaps.bitsflipper.com/scripts/php/get_data_list.php?limit=1";
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        response = httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception)
                    {
                        this.SystemStatusTb.Text = "Exception: Get HTTP Response Error!";
                        Speak("Error encountered while getting sensor updates from the database.");
                    }
                    httpClient.Dispose();
                }

                xdoc = XDocument.Parse(response);

                WTRadialGauge.Value = float.TryParse(xdoc.Descendants().First(node => node.Name == "wtemp").Value, out wtemp) ? wtemp : 0;
                ATRadialGauge.Value = float.TryParse(xdoc.Descendants().First(node => node.Name == "atemp").Value, out atemp) ? atemp : 0;
                RHRadialGauge.Value = float.TryParse(xdoc.Descendants().First(node => node.Name == "rh").Value, out rh) ? rh : 0;
                FRRadialGauge.Value = float.TryParse(xdoc.Descendants().First(node => node.Name == "fr").Value, out fr) ? fr : 0;
                PHRadialGauge.Value = float.TryParse(xdoc.Descendants().First(node => node.Name == "ph").Value, out ph) ? ph : 0;
                DORadialGauge.Value = float.TryParse(xdoc.Descendants().First(node => node.Name == "do2").Value, out do2) ? do2 : 0;

                tstamp = xdoc.Descendants().First(node => node.Name == "tstamp").Value;
                this.SystemStatusTb.Text = "Last Updated: " + tstamp;
            }
            catch (Exception)
            {
                this.SystemStatusTb.Text = "Exception: Update Gauges Error!";
                Speak("Unable to update Gauges. Please check the server or your internet connection.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LightBtn_Click(object sender, RoutedEventArgs e)
        {            
            if (LightSwitchPin.Read() == GpioPinValue.High)
            {
                LightSwitchPin.Write(GpioPinValue.Low);
                LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-off.png")), Stretch = Stretch.Fill };
                Speak("Lights OFF");
            }
            else
            {
                LightSwitchPin.Write(GpioPinValue.High);
                LightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/switch-on.png")), Stretch = Stretch.Fill };
                Speak("Lights ON");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaterPumpBtn_Click(object sender, RoutedEventArgs e)
        {
            Speak("Water pump automatic control is currently disabled.");            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Speak("Rebooting the system.");
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));		//Delay before restart after shutdown

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Speak("Shutting down.");
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Shutdown, TimeSpan.FromSeconds(1));		//Delay is not relevant to shutdown
        }
    }
}
