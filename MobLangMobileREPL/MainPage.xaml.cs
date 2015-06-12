using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture.Core;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MobLang;
using MobLang.Exceptions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace MobLangMobileREPL
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDisposable
    {
        private MobLang.Program _interpreter;
        private Task _task;
        private readonly CoreDispatcher _dispatcher;
        private double _defaultHistoryHeight;
        private readonly Brush _errorBrush = new SolidColorBrush(Colors.Red);
        private readonly Brush _reservedKeywordBrush = new SolidColorBrush(Colors.CornflowerBlue);
        private readonly Brush _numberBrush = new SolidColorBrush(Colors.LightSkyBlue);
        private readonly Brush _stringBrush = new SolidColorBrush(Colors.Aquamarine);
        public const string Prompt = "> ";
        public const string CodeStateName = "SavedCode";
        public const string Extension = ".mob";
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private static List<string> SavedCode 
        {
            get
            {
                object obj;
                SuspensionManager.SessionState.TryGetValue(CodeStateName, out obj);
                return obj as List<string>;
            }
            set { SuspensionManager.SessionState[CodeStateName] = value; }
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this._dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            InputPane.GetForCurrentView().Showing += KeyboardShow;
            InputPane.GetForCurrentView().Hiding += KeyboardHide;

            _defaultHistoryHeight = HistoryRow.ActualHeight;
        }

        private void KeyboardHide(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            HistoryRow.Height = new GridLength(1, GridUnitType.Star);
        }

        private void KeyboardShow(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _defaultHistoryHeight = Math.Max(_defaultHistoryHeight, HistoryRow.ActualHeight);
            HistoryRow.Height = new GridLength(_defaultHistoryHeight - args.OccludedRect.Height);
        }

        private void ReloadCode()
        {
             this._interpreter = new MobLang.Program();

            _interpreter.AddStd();
            _interpreter.AddImpure();
            
            _interpreter.Writer += async str =>
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    History.Children.Add(new TextBlock
                    {
                        Text = str,
                        FontFamily = Input.FontFamily,
                        FontStyle = FontStyle.Italic,
                        TextWrapping = TextWrapping.Wrap
                    });
                });
            };

            _interpreter.ErrorWriter += async str =>
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    History.Children.Add(new TextBlock
                    {
                        Text = str,
                        FontFamily = Input.FontFamily,
                        FontStyle = FontStyle.Italic,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = _errorBrush
                    });
                });
            };

            if (SavedCode != null)
            {    
                var token = _tokenSource.Token;
                Task.Factory.StartNew(async () =>
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var line in SavedCode)
                        {
                            token.ThrowIfCancellationRequested();
                            AddUserInput(line);
                            _interpreter.Execute(line, true);
                        }
                    });
                }, token);
            }
            else
            {
                SavedCode = new List<string>();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _defaultHistoryHeight = HistoryRow.ActualHeight;
            ReloadCode();
        }

        private void AddUserInput(string line)
        {
            var text = new RichTextBlock
            {
                FontFamily = Input.FontFamily,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Silver),
            };
            var paragraph = new Paragraph();
            
            var button = new TextBlock { Text = "Copy" };
            button.Tapped += (sender, args) => Input.Text = line;
            paragraph.Inlines.Add(new InlineUIContainer { Child = button });

            paragraph.Inlines.Add(new Run {Text = Prompt});

            try
            {
                var tokenizer = new MobLang.Parser.Tokenizer(line);
                while (tokenizer.Token != null)
                {
                    var run = new Run {Text = tokenizer.Token};
                    if (MobLang.Parser.Parser.ReservedKeywords.Contains(tokenizer.Token))
                        run.Foreground = _reservedKeywordBrush;
                    else if (tokenizer.Token.All(char.IsDigit))
                        run.Foreground = _numberBrush;
                    else
                        switch (tokenizer.Token[0])
                        {
                            case '\'':
                                run.Text += '\'';
                                run.Foreground = _stringBrush;
                                break;
                            case '"':
                                run.Text += '"';
                                run.Foreground = _stringBrush;
                                break;
                        }
                    run.Text += tokenizer.NextSpacing();
                    paragraph.Inlines.Add(run);

                    tokenizer.Advance();
                }
            }
            catch (MobLang.Exceptions.InvalidCharStringLiteralException)
            {
                paragraph.Inlines.Add(new Run
                {
                    Text = "<Lexer error>",
                    FontStyle = FontStyle.Oblique,
                    Foreground = _errorBrush
                });
            }

            text.Blocks.Add(paragraph);
            History.Children.Add(text);
        }

        private void SetComputing(bool computing)
        {
            Input.IsReadOnly = computing;

            CancelButton.IsEnabled = computing;
            EvaluateButton.IsEnabled = !computing;
        }

        private void Evaluate()
        {
            _tokenSource = new CancellationTokenSource();
            MobLang.Program.OnRecursion = null;

            var line = Input.Text;
            Input.Text = "";
            EvaluateAll(line);
        }

        private void EvaluateAll(string line)
        {
            foreach (var realLine in line.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                EvaluateLine(realLine);
        }

        private void EvaluateLine(string line)
        {
            switch (line)
            {
                case "#reset":
                    ResetCode();
                    return;
                case "#open":
                    FileOpen();
                    return;
                case "#save":
                    FileSave();
                    return;
            }

            AddUserInput(line);

            SetComputing(true);

            var token = _tokenSource.Token;
            MobLang.Program.RecursionDelegate checkIfCancelled = token.ThrowIfCancellationRequested;
            MobLang.Program.OnRecursion += checkIfCancelled;
            _task = Task.Factory.StartNew(() => _interpreter.Execute(line, true), token);
            _task.ContinueWith(async _ =>
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SetComputing(false);
                    HistoryScrollViewer.UpdateLayout();
                    HistoryScrollViewer.ChangeView(null, HistoryScrollViewer.ScrollableHeight, null);
                    SavedCode.Add(line);
                });
            }, token);
        }

        private void FileSave()
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                DefaultFileExtension = Extension,
                FileTypeChoices = { { "MobLang source file", new List<string> { Extension } } },
            };

            picker.PickSaveFileAndContinue();
        }

        private void FileOpen()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { Extension }
            };

            picker.PickSingleFileAndContinue();
        }

        public async void HandleFileOpen(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count != 1)
                return;

            var file = args.Files[0];

            await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            var code = await Windows.Storage.FileIO.ReadTextAsync(file);
            ResetCode();
            EvaluateAll(code);
        }

        public async void HandleFileSave(FileSavePickerContinuationEventArgs args)
        {
            if (args.File == null)
                return;

            Windows.Storage.CachedFileManager.DeferUpdates(args.File);
            await Windows.Storage.FileIO.WriteTextAsync(args.File, string.Join("\n\n", SavedCode));
            await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(args.File);
        }


        private void ResetCode()
        {
            SavedCode.Clear();
            History.Children.Clear();
        }

        private void EvaluateClicked(object sender, RoutedEventArgs e)
        {
            Evaluate();
        }

        private void InputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                Evaluate();
        }

        public void Dispose()
        {
            if(_tokenSource != null)
                _tokenSource.Dispose();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            if(_tokenSource != null)
                _tokenSource.Cancel(true);

            SetComputing(false);
        }
    }
}
