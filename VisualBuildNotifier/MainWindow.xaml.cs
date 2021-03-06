﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Client;
using Plenom.Components.Busylight.Sdk;
using VisualBuildNotifier.Core;
using VisualBuildNotifier.Core.Indicators;
using VisualBuildNotifier.Models;
using VisualBuildNotifier.Properties;
using VisualBuildNotifier.Services;
using VisualBuildNotifier.ViewModels;

namespace VisualBuildNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MainWindowViewModel _vm;
        public MainWindow()
        {
            _vm = new MainWindowViewModel(new ConfigManager());
            DataContext = _vm;

            InitializeComponent();            
        }

        private NotifyIcon _notifyIcon;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            
            _vm.LoadConfiguration();

            InitializeTrayIcon();

            _vm.StatusIndicators.AddRange(new IBuildStatusIndicator[] {
                new LoggingBuildStatusIndicator(), 
                new SystemTrayBuildStatusIndicator(_notifyIcon),
                new BusylightBuildStatusIndicator()
            });

            if (_vm.Blink1Enabled) {
                _vm.StatusIndicators.Add(new Blink1BuildStatusIndicator());
            }

            StateChanged += MainWindow_StateChanged;
            Closed += MainWindow_Closed;         
        }

        private void InitializeTrayIcon() {
            _notifyIcon = new NotifyIcon {
                Icon = Properties.Resources.Build,
                Visible = true
            };
            _notifyIcon.DoubleClick += _notifyIcon_DoubleClick;            
        }

        void _notifyIcon_DoubleClick(object sender, EventArgs e) {
            WindowState = (WindowState == WindowState.Minimized) ? WindowState.Normal : WindowState.Minimized;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (_notifyIcon != null) {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _vm.Cleanup();
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized) {
                ShowInTaskbar = false;
            } else {
                Topmost = true;
                ShowInTaskbar = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _vm.SelectServer();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e) {
            _vm.SaveConfiguration();

            WindowState = WindowState.Minimized;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            _vm.DiscardPendingConfiguration();
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            _vm.RefreshBuildDefinitions();
        }
    }
}
