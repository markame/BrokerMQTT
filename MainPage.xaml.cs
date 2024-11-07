using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MQTTnet;
using MQTTnet.Client;
namespace BrokerMQTT
{
    public partial class MainPage : ContentPage
    {

        private IMqttClient mqttClient;
        public MainPage()
        {
            InitializeComponent();
            InitializeMqttClient();
        }
        private void InitializeMqttClient()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            // Evento disparado quando uma mensagem é recebida
            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagesLabel.Text += $"\nTópico: {e.ApplicationMessage.Topic} - Mensagem: {message}";
                });
            };

            // Evento disparado quando conectado com sucesso
            mqttClient.ConnectedAsync += async e =>
            {
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("/Markame/temp").Build());
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagesLabel.Text += "\nConectado e inscrito no tópico '/Markame/temp'.";
                });
            };

            // Evento disparado quando desconectado
            mqttClient.DisconnectedAsync += async e =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagesLabel.Text += "\nDesconectado do broker MQTT.";
                });
            };
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId("maui-client-" + Guid.NewGuid())
                .WithTcpServer("test.mosquitto.org", 1883)  // Broker e porta
                .WithCleanSession()
                .Build();

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagesLabel.Text += $"\nErro ao conectar: {ex.Message}";
                });
            }
        }
    }
}