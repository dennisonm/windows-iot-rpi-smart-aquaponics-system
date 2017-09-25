﻿#pragma checksum "H:\Dropbox\Projects\IoT\Windows IoT\RPI\windows-iot-rpi-smart-aquaponics-system\SAPv2r800x480\Gauges.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6F5A92A1B73374700ED6379EF7948B81"
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
    partial class Gauges : 
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
                    this.HomeBtn = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 308 "..\..\..\Gauges.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.HomeBtn).Click += this.HomeBtn_Click;
                    #line default
                }
                break;
            case 3:
                {
                    this.SettingsBtn = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 322 "..\..\..\Gauges.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.SettingsBtn).Click += this.SettingsBtn_Click;
                    #line default
                }
                break;
            case 4:
                {
                    this.FlowRate = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 5:
                {
                    this.pH = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 6:
                {
                    this.DissolvedOxygen = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7:
                {
                    this.FRRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 8:
                {
                    this.PHRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 9:
                {
                    this.DORadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 10:
                {
                    this.WaterTemp = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 11:
                {
                    this.AmbientTemp = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 12:
                {
                    this.RelativeHumidity = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 13:
                {
                    this.ATRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 14:
                {
                    this.RHRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 15:
                {
                    this.WTRadialGauge = (global::Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge)(target);
                }
                break;
            case 16:
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
