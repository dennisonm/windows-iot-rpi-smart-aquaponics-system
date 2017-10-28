using System;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
using Windows.Media.SpeechSynthesis;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Email;

using LightBuzz.SMTP;


namespace SAP
{
    public sealed partial class MainPage : Page
    {
        // Clock                
        private DispatcherTimer DispatcherClockTimer;                                   // DispatcherTimer for the clock 
        private TimeZoneInfo remoteTimeZone;
        private static bool broadcasted = false;                                        // flag to avoid broadcasting time indefinitely

        // Automatic Switches         
        private DispatcherTimer DispatcherControlTimer;                                 // DispatcherTimer for aquaponics switches
        private static bool gpioDevice = false;                                         
        private static bool firstTick = true;

        // Feeder 
        private static bool fed = true;
        private static bool fishIsHungry = false;
        private static bool feedmelblvisibility = false;

        // Alarms
        private BitmapImage GreenLED = new BitmapImage(new Uri("ms-appx:///Assets/Green-LED.png"));
        private BitmapImage RedLED = new BitmapImage(new Uri("ms-appx:///Assets/Red-LED.png"));
        private static string statusLED = "red";

        // Gauges
        private DispatcherTimer DispatcherUpdateGaugeTimer = new DispatcherTimer();     // Dispatcher timer for gauge update
        private static int updateGaugesErrorCounter = 0;
        private static bool sensorOutOfSpec = false;

        // Initializes a new instance of the XDocument class (represents an XML document)
        private XDocument xdoc = new XDocument();

        // Instantiate a new instance of MediaElement
        // Represents an object that renders audio and video to the display
        private MediaElement mediaElement = new MediaElement();

        // Network
        private static int httpResponseErrorCounter = 0;

        // RPI GPIO Settings (Pin Assignments)
        private const int GrowLightSwitch_PIN = 24;
        private const int TankLightSwitch_PIN = 23;
        private const int WaterPumpSwitch_PIN = 26;
        private GpioPin GrowLightSwitchPin;
        private GpioPin TankLightSwitchPin;
        private GpioPin WaterPumpSwitchPin;

        // SMTP Setup
        private const string SMTP_SERVER = SMTP.SMTP_SERVER;          // your SMTP server
        private const string SMTP_USER = SMTP.SMTP_USER;              // your SMTP username
        private const string SMTP_PASSWORD = SMTP.SMTP_PASSWORD;      // your SMTP password
        private const string MAIL_RECIPIENT = SMTP.MAIL_RECIPIENT;    // mail recipient
        private const bool SMTP_SSL = true;                           // send email over SSL
        private const int SMTP_PORT = 465;                            // your SMTP port
        private static bool emailSuccessfullySent = false;

        // Device Name
        private const string DeviceName = "My Indoor Aquaponics System";

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize Status TextBlock            
            this.SystemStatusTb.Text = "Initializing...";

            // Set default Fee Me button status
            FeedBtn.Background = null;
            FeedMeLbl.Visibility = Visibility.Collapsed;

            // DispatcherTimer setup for the Date, Clock and Blinking Alarm Notification
            DispatcherClockTimer = new DispatcherTimer();
            DispatcherClockTimer.Tick += DispatcherClockTimer_Tick;
            DispatcherClockTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherClockTimer.Start();

            // DispatcherTimer for aquaponics switches
            DispatcherControlTimer = new DispatcherTimer();
            DispatcherControlTimer.Tick += DispatcherControlTimer_Tick;
            DispatcherControlTimer.Interval = TimeSpan.FromSeconds(1);

            // DispatcherTimer setup for the Sensor Reading Updates
            DispatcherUpdateGaugeTimer.Interval = TimeSpan.FromSeconds(60);
            DispatcherUpdateGaugeTimer.Tick += DispatcherUpdateGaugeTimer_Tick;
            UpdateGauges();
            DispatcherUpdateGaugeTimer.Start();

            // Update Location TextBlock                       
            if (GetLocationByIPAddress() == null || GetLocationByIPAddress() == "")
            {
                this.SystemStatusTb.Text = "Error getting location by IP Address!";
                this.LocalLocationLbl.Text = "Singapore, Singapore";
                this.RemoteLocationLbl.Text = "Sydney";
                Speak(
                    "I am having problem getting your location." +
                    " " +
                    "Please check your network.");
            }
            else
                this.LocalLocationLbl.Text = GetLocationByIPAddress();

            if (GetLocationByIPAddress().Contains("Singapore"))
            {
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                RemoteLocationLbl.Text = "Sydney";
            }
            else if (GetLocationByIPAddress().Contains("Australia"))
            {
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                RemoteLocationLbl.Text = "Cebu";
            }
            else
            {
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                RemoteLocationLbl.Text = "Sydney";
            }

            // Initialize GPIO
            InitGPIO();

            SendMail("Notification from " + DeviceName, "System started on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt"));
        }

        /// <summary>
        /// Speech Synthesizer
        /// Text to Speech method (Speak)
        /// </summary>
        private async void Speak(string text)
        {
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
        /// Check and switch ON/OFF the switches
        /// </summary>
        private void DispatcherControlTimer_Tick(object sender, object e)
        {
            if (gpioDevice)
            {
                // Grow Lights are always ON by default                
                if (GrowLightSwitchPin.Read() == GpioPinValue.Low)
                {
                    GrowLightSwitchPin.Write(GpioPinValue.High);
                    GrowLightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Switch-On.png")), Stretch = Stretch.Fill };
                    //Speak("Grow Lights ON");
                }

                // Tank lights
                // ON from 7:00pm to 11.59pm
                if ((DateTime.Now.Hour > 18) && (DateTime.Now.Hour <= 23))
                {
                    // Switch Grow Lights ON
                    if (TankLightSwitchPin.Read() == GpioPinValue.Low)
                    {
                        TankLightSwitchPin.Write(GpioPinValue.High);
                        TankLightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Switch-On.png")), Stretch = Stretch.Fill };
                        Speak("Tank Lights ON");
                    }
                }
                else
                {
                    if (TankLightSwitchPin.Read() == GpioPinValue.High)
                    {
                        TankLightSwitchPin.Write(GpioPinValue.Low);
                        TankLightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Switch-Off.png")), Stretch = Stretch.Fill };
                        Speak("Tank Lights OFF");

                    }
                }

                // Pumps are always ON by default
                if (WaterPumpSwitchPin.Read() == GpioPinValue.Low)
                {
                    WaterPumpSwitchPin.Write(GpioPinValue.High);
                    WaterPumpBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Switch-On.png")), Stretch = Stretch.Fill };
                    //Speak("Water Pumps ON");
                }
            }

            // Manual Feeding
            if (((DateTime.Now.Hour > 5) && !fed) || ((DateTime.Now.Hour > 17) && !fed))
            {
                if (FeedBtn.Background == null)
                    FeedBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Red-Fish.png")), Stretch = Stretch.Fill };
                FeedMeLbl.Visibility = Visibility.Visible;
                fishIsHungry = true;
            }
            if (fed)
            {
                FeedBtn.Background = null;
                FeedMeLbl.Visibility = Visibility.Collapsed;
                fishIsHungry = false;
            }

            if (((DateTime.Now.Hour == 6) && fed) || ((DateTime.Now.Hour == 18) && fed))
                fed = false;

            // Set interval to 60s after the first tick
            if (firstTick)
            {
                DispatcherControlTimer.Interval = TimeSpan.FromSeconds(60);
                firstTick = false;
            }
        }

        /// <summary>
        /// Click after feeding to hide the Feed Me button and label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FeedBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SystemStatusTb.Text = "Manually fed the fish :)";
            SendMail("Notification from " + DeviceName, "Manually fed the fish on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt"));
            fed = true;
            FeedBtn.Background = null;
            FeedMeLbl.Visibility = Visibility.Collapsed;
            feedmelblvisibility = false;
        }

        /// <summary>
        /// Updates the current time and date display
        /// </summary>
        private void DispatcherClockTimer_Tick(object sender, object e)
        {
            // Display
            this.LocalTimeLbl.Text = DateTime.Now.ToString("h:mm");
            this.LocalDateLbl.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            this.LocalTimeSecLbl.Text = DateTime.Now.ToString("ss");
            this.LocalTimeAMPMLbl.Text = (DateTime.Now.Hour >= 12) ? "PM" : "AM";

            // Broadcast time every hour
            if (DateTime.Now.Minute == 0)
            {
                if (broadcasted == false && DateTime.Now.Hour > 5)
                {
                    if (DateTime.Now.Hour <= 12)
                        Speak("It's " + DateTime.Now.Hour + " o'clock!");
                    else
                        Speak("It's " + (DateTime.Now.Hour - 12) + " o'clock!");
                    broadcasted = true; // flag to avoid broadcasting the time over and over again
                }
            }
            else if (DateTime.Now.Minute == 1)
                broadcasted = false;    // reset flag

            DateTime remoteTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, remoteTimeZone);
            this.RemoteTimeLbl.Text = Convert.ToString(remoteTime.ToString("h:mm tt"));
            this.RemoteDateLbl.Text = Convert.ToString(remoteTime.ToString("ddd, MMM dd"));

            if (fishIsHungry || sensorOutOfSpec)
            {
                if (statusLED == "green")
                {
                    SystemStatusIndicator.Source = RedLED;
                    statusLED = "red";
                }
                if (fishIsHungry)
                {
                    FeedMeLbl.Text = (feedmelblvisibility == true) ? "" : "Feed Me!";
                    feedmelblvisibility = !feedmelblvisibility;
                }
                else
                {
                    FeedMeLbl.Text = "";
                    feedmelblvisibility = false;
                }
            }
            else
            {
                if (statusLED == "red")
                {
                    SystemStatusIndicator.Source = GreenLED;
                    statusLED = "green";
                }
            }
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
            catch (Exception)
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
            catch (Exception)
            {
                this.SystemStatusTb.Text = "Exception: Get Location By IP Address Error!";
            }
            return location.Trim();
        }

        /// <summary>
        /// Initialize RPI GPIO Pins
        /// </summary>
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Check GPIO controller
            if (gpio == null)
            {
                SystemStatusTb.Text = "Status: There is no GPIO controller on this device";
                Speak("There is no GPIO controller on this device.");
                gpioDevice = false;
                DispatcherControlTimer.Start();
                return;
            }

            gpioDevice = true;

            // Configure TankLightSwitchPin
            TankLightSwitchPin = gpio.OpenPin(TankLightSwitch_PIN);
            TankLightSwitchPin.Write(GpioPinValue.Low);
            TankLightSwitchPin.SetDriveMode(GpioPinDriveMode.Output);

            // Configure GrowLightSwitchPin
            GrowLightSwitchPin = gpio.OpenPin(GrowLightSwitch_PIN);
            GrowLightSwitchPin.Write(GpioPinValue.Low);
            GrowLightSwitchPin.SetDriveMode(GpioPinDriveMode.Output);

            // Configure WaterPumpSwitchPin
            WaterPumpSwitchPin = gpio.OpenPin(WaterPumpSwitch_PIN);
            WaterPumpSwitchPin.Write(GpioPinValue.Low);
            WaterPumpSwitchPin.SetDriveMode(GpioPinDriveMode.Output);

            DispatcherControlTimer.Start();
        }

        /// <summary>
        /// Asynchronously updates the gauges
        /// </summary>
        private async void DispatcherUpdateGaugeTimer_Tick(object sender, object e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateGauges());
        }

        /// <summary>
        /// Updates the gauges
        /// </summary>
        private void UpdateGauges()
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
                        ++httpResponseErrorCounter;
                        this.SystemStatusTb.Text = "Exception: Get HTTP Response Error!";
                        Speak("Error encountered while getting sensor updates from the database.");
                        if (httpResponseErrorCounter == 4)
                            Speak("Will try to establish connection again. If still unsuccessful, the system will be rebooted.");
                        if (httpResponseErrorCounter == 5)
                        {
                            Speak("Rebooting the system.");
                            emailSuccessfullySent = false;
                            SendMail("Notification from " + DeviceName, "System restarted on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt") + "due to persistent network issue.");
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            while (!emailSuccessfullySent && (sw.ElapsedMilliseconds < 5000)) ;
                            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));
                        }
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
                ++updateGaugesErrorCounter;
                this.SystemStatusTb.Text = "Exception: Update Gauges Error!";
                Speak("Unable to update Gauges. Please check the server or your internet connection.");
                if (updateGaugesErrorCounter == 4)
                    Speak("Will try to update again. If still unsuccessful, the system will be rebooted.");
                if (updateGaugesErrorCounter == 5)
                {
                    Speak("Rebooting the system.");
                    emailSuccessfullySent = false;
                    SendMail("Notification from " + DeviceName, "System restarted on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt") + ". Unable to update gauges.");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (!emailSuccessfullySent && (sw.ElapsedMilliseconds < 5000)) ;
                    Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));
                }
            }
        }

        /// <summary>
        /// Manually turning ON/OFF the Tank Lights.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TankLightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (gpioDevice)
            {
                if (TankLightSwitchPin.Read() == GpioPinValue.High)
                {
                    TankLightSwitchPin.Write(GpioPinValue.Low);
                    TankLightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Switch-Off.png")), Stretch = Stretch.Fill };
                    Speak("Tank Lights OFF");
                }
                else
                {
                    TankLightSwitchPin.Write(GpioPinValue.High);
                    TankLightBtn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, "Assets/Switch-On.png")), Stretch = Stretch.Fill };
                    Speak("Tank Lights ON");
                }
            }
            else
            {
                SystemStatusTb.Text = "Status: There is no GPIO controller on this device";
                Speak("There is no GPIO controller on this device.");
            }

        }

        /// <summary>
        /// Manually turning ON/OFF the Grow Lights.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrowLightBtn_Click(object sender, RoutedEventArgs e)
        {
            Speak("Grow Lights control is currently disabled.");
        }

        /// <summary>
        /// Manually turning ON/OFF the water pump.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaterPumpBtn_Click(object sender, RoutedEventArgs e)
        {
            Speak("Water pump control is currently disabled.");
        }

        /// <summary>
        /// Shuts down, then restarts the device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Speak("Rebooting the system.");
            emailSuccessfullySent = false;
            SendMail("Notification from " + DeviceName, "System restarted on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt"));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!emailSuccessfullySent && (sw.ElapsedMilliseconds < 5000)) ;
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));		//Delay before restart after shutdown
        }

        /// <summary>
        /// Shuts down the device without restarting it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Speak("Shutting down.");
            emailSuccessfullySent = false;
            SendMail("Notification from " + DeviceName, "System shutdown initiated on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt"));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!emailSuccessfullySent && (sw.ElapsedMilliseconds < 5000)) ;
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Shutdown, TimeSpan.FromSeconds(1));		//Delay is not relevant to shutdown            
        }

        /// <summary>
        /// Send test email
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMail_Click(object sender, RoutedEventArgs e)
        {
            Speak("Sending test email to " + MAIL_RECIPIENT);
            SendMail("Test Email Notification", "Test email from " + GetPublicIPAddress() + " on " + DateTime.Now.ToString("MMMM dd, yyyy") + ", at " + DateTime.Now.ToString("hh:mm tt"));
        }

        /// <summary>
        /// Asynchronously Send email
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private async void SendMail(string subject, string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(SMTP_SERVER, SMTP_PORT, SMTP_SSL, SMTP_USER, SMTP_PASSWORD))
                {
                    EmailMessage emailMessage = new EmailMessage();

                    emailMessage.To.Add(new EmailRecipient(MAIL_RECIPIENT));
                    emailMessage.Subject = subject;
                    emailMessage.Body = body;

                    await client.SendMailAsync(emailMessage);
                    emailSuccessfullySent = true;
                    this.SystemStatusTb.Text = "Email was sent successfully!";
                }
            }
            catch
            {
                emailSuccessfullySent = false;
                this.SystemStatusTb.Text = "Failed to send email.";
            }
        }

        /// <summary>
        /// About the app
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private async void About_Click(object sender, RoutedEventArgs e)
        {
            var assyVersion = typeof(App).GetTypeInfo().Assembly.GetName().Version;            

            ContentDialog about = new ContentDialog()
            {
                Title = "About SAP",
                Content = "Smart Aquaponics System Prototype\n" + "Developed by Myron Richard Dennison\n" + "Version: " + assyVersion.ToString(),                
                CloseButtonText = "Close"
            };

            await about.ShowAsync();            
        }
    }
}
