﻿#pragma checksum "H:\Dropbox\Projects\IoT\Windows IoT\RPI\windows-iot-rpi-smart-aquaponics-system\SAPv2r800x480\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "AF15879CFDDD302A63ADBCC1BCD88AC9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SAP
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.SystemStatusTb = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 2:
                {
                    this.WaterTemp = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 3:
                {
                    this.AmbientTemp = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 4:
                {
                    this.RelativeHumidity = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 5:
                {
                    this.FlowRate = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 6:
                {
                    this.pH = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7:
                {
                    this.DissolvedOxygen = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 8:
                {
                    this.ATRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 9:
                {
                    this.RHRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 10:
                {
                    this.WTRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 11:
                {
                    this.FRRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 12:
                {
                    this.PHRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 13:
                {
                    this.DORadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 14:
                {
                    this.Lbl_Date = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 15:
                {
                    this.Lbl_Location = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 16:
                {
                    this.Lbl_Time = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 17:
                {
                    this.TwelveHrCb = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    #line 141 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.TwelveHrCb).Checked += this.TwelveHrCb_Checked;
                    #line default
                }
                break;
            case 18:
                {
                    this.TwentyFourHrCb = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    #line 146 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.TwentyFourHrCb).Checked += this.TwentyFourHrCb_Checked;
                    #line default
                }
                break;
            case 19:
                {
                    this.LED = (global::Windows.UI.Xaml.Shapes.Ellipse)(target);
                }
                break;
            case 20:
                {
                    this.Lbl_LightStatus = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 21:
                {
                    this.pageTitle = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

