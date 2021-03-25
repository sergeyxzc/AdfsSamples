using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdfsWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AdfsTokenService _adfsTokenService;

        public MainWindow()
        {
            InitializeComponent();

            _adfsTokenService = new AdfsTokenService(
                "https://adfs-next.gkbaltbet.local/adfs/",
                "http://wpfclienturi/",
                "0f4c9640-adb1-47bf-a497-370caf2008fc",
                () => new WindowInteropHelper(this).Handle);
        }

        private async void Signin_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _adfsTokenService.Init();

                var token = await _adfsTokenService.GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                    return;

                var handler = new JwtSecurityTokenHandler();

                var securityToken = handler.ReadToken(token);

                ResultText.Text = securityToken.ToString()!;
            }
            catch (Exception exception)
            {
                ResultText.Text = exception.Message;
            }
        }
    }
}
