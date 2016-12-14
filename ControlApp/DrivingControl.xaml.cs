using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using SpheroHelperLibrary;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ControlApp
{
  public sealed partial class DrivingControl : UserControl
  {
    private Dictionary<string, Action> namedActions;

    public DrivingControl()
    {
      this.InitializeComponent();

      this.namedActions = new Dictionary<string, Action>()
      {
        ["go"] = () => this.Ball.Forward(5.0f),
        ["stop"] = () => this.Ball.Stop(),
        ["red"] = () => this.Ball.SetColour(0xFF, 0x00, 0x00),
        ["green"] = () => this.Ball.SetColour(0x00, 0xFF, 0x00),
        ["blue"] = () => this.Ball.SetColour(0x00, 0x00, 0xFF),
        ["left"] = () => this.Ball.IncrementalAnticlockwiseTurn(10),
        ["right"] = () => this.Ball.IncrementalClockwiseTurn(10),
        ["black"] = () => this.Ball.SetColour(0x00, 0x00, 0x00),
        ["light on"] = () => this.Ball.SwitchBacklightLED(true),
        ["light off"] = () => this.Ball.SwitchBacklightLED(false),
        ["show off"] = () => this.Ball.FlashRedGreenBlueAsync(TimeSpan.FromSeconds(3))
      };
    }

    internal async void SetBall(SpheroBall ball)
    {
      this.Ball = ball;
      await this.StartListeningForSpeechAsync();
    }
    SpheroBall Ball { get; set; }


    void OnRotationChanged(object sender,
      RotarySelectorValueChangedEventArgs e)
    {
      this.Ball.ChangeHeading(e.NewValue);
    }
    void OnSpeedChanged(object sender,
      RangeBaseValueChangedEventArgs e)
    {
      this.Ball.Forward(e.NewValue);
    }
    void OnColour(byte red, byte green, byte blue)
    {
      this.Ball.SetColour(red, green, blue);
    }
    void OnRed(object sender, RoutedEventArgs e)
    {
      this.OnColour(0xFF, 0x00, 0x00);
    }
    void OnGreen(object sender, RoutedEventArgs e)
    {
      this.OnColour(0x00, 0xFF, 0x00);
    }
    void OnBlue(object sender, RoutedEventArgs e)
    {
      this.OnColour(0x00, 0x00, 0xFF);
    }

    async Task StartListeningForSpeechAsync()
    {
      // make a recognizer
      this.recognizer = new SpeechRecognizer();
      this.synthesizer = new SpeechSynthesizer();

      // we feed it the key from that dictionary as a set of phrases to
      // listen for. We can also use speech grammars and dictation.
      this.recognizer.Constraints.Add(new SpeechRecognitionListConstraint(
        this.namedActions.Keys));

      await this.recognizer.CompileConstraintsAsync();

      // when a result is recognised...      
      this.recognizer.ContinuousRecognitionSession.ResultGenerated += async (s, e) =>
      {
        // what text did the recognizer hear?
        var commandTextHeard = e.Result?.Text;

        // If it's in our list, execute it. Otherwise, ignore.
        if (this.namedActions.ContainsKey(commandTextHeard))
        {
          this.namedActions[commandTextHeard]();

          // Say what we just did.
          await this.SpeakAsync($"ok, made it {commandTextHeard}");
        }
        this.recognizer.ContinuousRecognitionSession.Resume();
      };
      // set the async recognition running continuously, asking for
      // a pause when a result is recognised.
      await this.recognizer.ContinuousRecognitionSession.StartAsync(
        SpeechContinuousRecognitionMode.PauseOnRecognition);
    }
    async Task SpeakAsync(string text)
    {
      // this is hacky, was added in a hurry!
      await Dispatcher.RunAsync(
        Windows.UI.Core.CoreDispatcherPriority.Normal,
        async () =>
        {
          MediaElement media = new MediaElement();

          var stream = await this.synthesizer.SynthesizeTextToStreamAsync(text);

          media.SetSource(stream, string.Empty);
          media.Play();
          media.MediaEnded += (sender, args) =>
          {
            stream.Dispose();
          };
        }
      );
    }
    SpeechRecognizer recognizer;
    SpeechSynthesizer synthesizer;

  }
}
