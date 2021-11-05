using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;

namespace OsuSkinMixer
{
    public class Main : Control
    {
        private const string VERSION = "v1.2";

        private readonly OptionInfo[] Options = new OptionInfo[]
        {
            new OptionInfo
            {
                Name = "Interface",
                IsAudio = false,
                IncludeSkinIniProperties = new Dictionary<string, string[]>()
                {
                    {
                        "General", new string[]
                        {
                            "AllowSliderBallTint",
                            "ComboBurstRandom",
                        }
                    },
                    {
                        "Colours", new string[]
                        {
                            "InputOverlayText",
                            "MenuGlow",
                            "SongSelectActiveText",
                            "SongSelectInactiveText",
                        }
                    },
                    {
                        "Fonts", new string[]
                        {
                            "ScorePrefix",
                            "ScoreOverlap",
                            "ComboPrefix",
                            "ComboOverlap",
                        }
                    },
                },
                IncludeFileNames = new string[]
                {
                    "arrow-pause",
                    "arrow-warning",
                    "button-left",
                    "button-middle",
                    "button-right",
                    "count-1",
                    "count-2",
                    "count-3",
                    "fail-background",
                    "go",
                    "inputoverlay*",
                    "menu*",
                    "mode-*",
                    "options-offset-tick",
                    "pause*",
                    "play*",
                    "ranking*",
                    "ready",
                    "score-*",
                    "scorebar-*",
                    "scoreentry-*",
                    "section-*",
                    "selection-*",
                    "star",
                    "welcome_text",
                    // For some reason whitecat 2.1 skin uses this prefix instead of "score-[0-9].png",
                    // this is just a hotfix for that since it's a popular skin.
                    "numbers-*",
                },
            },
            new OptionInfo
            {
                Name = "Gameplay",
                IsAudio = false,
                IncludeSkinIniProperties = new Dictionary<string, string[]>()
                {
                    {
                        "General", new string[]
                        {
                            "AnimationFramerate",
                            "HitCircleOverlayAboveNumber",
                            "SliderBallFlip",
                            "SpinnerFadePlayfield",
                            "SpinnerNoBlink",
                        }
                    },
                    {
                        "Colours", new string[]
                        {
                            "Combo1",
                            "Combo2",
                            "Combo3",
                            "Combo4",
                            "Combo5",
                            "Combo6",
                            "Combo7",
                            "Combo8",
                            "SliderBall",
                            "SliderBorder",
                            "SliderTrackOverride",
                            "SpinnerBackground",
                            "StarBreakAdditive",
                        }
                    },
                    {
                        "Fonts", new string[]
                        {
                            "HitCirclePrefix",
                            "HitCircleOverlap",
                        }
                    },
                    {
                        "CatchTheBeat", new string[]
                        {
                            "HyperDash",
                            "HyperDashFruit",
                            "HyperDashAfterImage",
                        }
                    },
                },
                IncludeFileNames = new string[]
                {
                    "approachcircle",
                    "comboburst*",
                    "default-*",
                    "followpoint*",
                    "fruit-*",
                    "hit*",
                    "lighting*",
                    "mania*",
                    "masking-border",
                    "particle*",
                    "reversearrow",
                    "sliderb*",
                    "sliderendcircle*",
                    "sliderfollowcircle",
                    "sliderpoint*",
                    "sliderstartcircle*",
                    "spinner-*",
                    "star2",
                    "taiko*",
                    "target*",
                    "skin",
                },
            },
            new OptionInfo
            {
                Name = "Cursor",
                IsAudio = false,
                IncludeSkinIniProperties = new Dictionary<string, string[]>()
                {
                    {
                        "General", new string[]
                        {
                            "CursorCentre",
                            "CursorExpand",
                            "CursorRotate",
                            "CursorTrailRotate",
                        }
                    },
                },
                IncludeFileNames = new string[]
                {
                    "cursor*",
                }
            },
            new OptionInfo
            {
                Name = "Hitsounds",
                IsAudio = true,
                IncludeSkinIniProperties = new Dictionary<string, string[]>()
                {
                    {
                        "General", new string[]
                        {
                            "CustomComboBurstSounds",
                            "LayeredHitSounds",
                            "SpinnerFrequencyModulate",
                        }
                    },
                },
                IncludeFileNames = new string[]
                {
                    "combobreak",
                    "comboburst",
                    "drum-*",
                    "nightcore-*",
                    "normal-*",
                    "pippidon*",
                    "soft-*",
                    "spinner-*",
                },
            },
            new OptionInfo
            {
                Name = "Menu Sounds",
                IsAudio = true,
                IncludeFileNames = new string[]
                {
                    "applause",
                    "back-button-click",
                    "back-button-hover",
                    "check-off",
                    "check-on",
                    "click-close",
                    "click-short-confirm",
                    "click-short",
                    "count1s",
                    "count2s",
                    "count3s",
                    "failsound",
                    "gos",
                    "heartbeat",
                    "key*",
                    "match*",
                    "menu*",
                    "metronomelow",
                    "multi-skipped",
                    "pause*",
                    "readys",
                    "section*",
                    "seeya",
                    "select-*",
                    "shutter",
                    "sliderbar",
                    "welcome",
                },
            },
        };

        private Dialog Dialog;
        private Button CreateSkinButton;
        private LineEdit SkinNameEdit;
        private LinkButton UpdateLink;

        private string SkinsFolder { get; set; } = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/osu!/Skins";

        private string[] Skins { get; set; } = Array.Empty<string>();

        public override void _Ready()
        {
            OS.SetWindowTitle("osu! skin mixer");

            Dialog = GetNode<Dialog>("Dialog");
            CreateSkinButton = GetNode<Button>("ButtonsCenterContainer/HBoxContainer/CreateSkinButton");
            SkinNameEdit = GetNode<LineEdit>("ButtonsCenterContainer/HBoxContainer/SkinNameEdit");
            UpdateLink = GetNode<LinkButton>("UpdateLink");

            CreateSkinButton.Connect("pressed", this, "_CreateSkinButtonPressed");
            UpdateLink.Connect("pressed", this, "_UpdateLinkPressed");

            if (!CreateOptionButtons())
            {
                Dialog.TextInput(
                    text: "Couldn't find your skins folder, please set it below.",
                    func: p =>
                    {
                        SkinsFolder = p;
                        return CreateOptionButtons();
                    },
                    defaultText: SkinsFolder);
            }

            CheckForUpdates();
        }

        public void _UpdateLinkPressed()
        {
            OS.ShellOpen("https://github.com/rednir/OsuSkinMixer/releases/latest");
        }

        public void _CreateSkinButtonPressed() => CreateSkin();

        private void CreateSkin()
        {
            string newSkinName = SkinNameEdit.Text;

            if (string.IsNullOrWhiteSpace(newSkinName))
            {
                Dialog.Alert("Set a name for the new skin first.");
                return;
            }

            if (Directory.Exists(SkinsFolder + "/" + newSkinName))
            {
                Dialog.Question(
                    text: $"A skin with that name already exists. Replace it?\n\nThis will permanently remove '{newSkinName}'",
                    action: b =>
                    {
                        if (b)
                        {
                            Directory.Delete(SkinsFolder + "/" + newSkinName, true);
                            runCont();
                        }
                    });
                return;
            }

            runCont();

            void runCont()
            {
                CreateSkinButton.Disabled = true;
                CreateSkinButton.Text = "Working...";

                Task.Run(cont)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Dialog.Alert($"Something went wrong.\n\n{t.Exception.Message}");

                        CreateSkinButton.Disabled = false;
                        CreateSkinButton.Text = "Create skin";
                    });
            }

            void cont()
            {
                var newSkinIni = new SkinIni(newSkinName, "osu! skin mixer by rednir");
                var newSkinDir = Directory.CreateDirectory(SkinsFolder + "/" + newSkinName);

                foreach (var option in Options)
                {
                    var node = GetNode<OptionButton>(option.NodePath);

                    // User wants default skin elements to be used.
                    if (node.GetSelectedId() == 0)
                        continue;

                    var skindir = new DirectoryInfo(SkinsFolder + "/" + node.Text);
                    var skinini = new SkinIni(File.ReadAllText(skindir + "/skin.ini"));

                    foreach (var file in skindir.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        string filename = Path.GetFileNameWithoutExtension(file.Name);
                        string extension = Path.GetExtension(file.Name);

                        foreach (string optionFilename in option.IncludeFileNames)
                        {
                            // Check for file name match.
                            if (filename.Equals(optionFilename, StringComparison.OrdinalIgnoreCase) || filename.Equals(optionFilename + "@2x", StringComparison.OrdinalIgnoreCase)
                                || (optionFilename.EndsWith("*") && filename.StartsWith(optionFilename.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)))
                            {
                                // Check for file type match.
                                if (
                                    ((extension == ".png" || extension == ".jpg" || extension == ".ini") && !option.IsAudio)
                                    || ((extension == ".mp3" || extension == ".ogg" || extension == ".wav") && option.IsAudio)
                                )
                                {
                                    if (!File.Exists(newSkinDir.FullName + "/" + file.Name))
                                        file.CopyTo(newSkinDir.FullName + "/" + file.Name);
                                }
                            }
                        }
                    }

                    foreach (var section in skinini.Sections)
                    {
                        // Only look into ini sections that are specified in the IncludeSkinIniProperties.
                        if (!option.IncludeSkinIniProperties.ContainsKey(section.Name))
                            continue;

                        foreach (var pair in section)
                        {
                            // Only copy over ini properties that are specified in the IncludeSkinIniProperties.
                            if (option.IncludeSkinIniProperties[section.Name].Contains(pair.Key))
                            {
                                newSkinIni.Sections.Find(s => s.Name == section.Name).Add(
                                    key: pair.Key,
                                    // All of the skin elements will be in skin directory root, so get rid of child directories in path names.
                                    value: pair.Key.Contains("Prefix") && pair.Value.Contains('/') ? pair.Value.Split('/').Last() : pair.Value);
                            }
                        }
                    }
                }

                File.WriteAllText(newSkinDir.FullName + "/skin.ini", newSkinIni.ToString());

                Dialog.Alert($"Created skin '{newSkinName}'.\n\nYou might need to press Ctrl+Shift+Alt+S in-game for the skin to appear.");
            }
        }

        private bool CreateOptionButtons()
        {
            if (!Directory.Exists(SkinsFolder))
                return false;

            var vbox = GetNode("OptionsContainer/CenterContainer/HBoxContainer");
            var directory = new DirectoryInfo(SkinsFolder);
            Skins = directory.EnumerateDirectories().Select(d => d.Name).OrderBy(n => n).ToArray();

            foreach (var option in Options)
            {
                var hbox = GetNode<HBoxContainer>("OptionTemplate").Duplicate();
                var label = hbox.GetChild<Label>(0);
                var optionButton = hbox.GetChild<OptionButton>(1);

                hbox.Name = option.Name;
                label.Text = option.Name;

                optionButton.AddItem("<< use default skin >>");
                optionButton.AddSeparator();
                foreach (var skin in Skins)
                    optionButton.AddItem(skin);

                vbox.AddChild(hbox);
            }

            return true;
        }

        private void CheckForUpdates()
        {
            var req = GetNode<HTTPRequest>("HTTPRequest");
            req.Connect("request_completed", this, "_UpdateRequestCompleted");
            req.Request("https://api.github.com/repos/rednir/OsuSkinMixer/releases/latest", new string[] { "User-Agent: OsuSkinMixer" });
        }

        public void _UpdateRequestCompleted(int result, int response_code, string[] headers, byte[] body)
        {
            if (result != 0)
                return;

            try
            {
                string latest = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(body))["tag_name"].ToString();
                if (latest != VERSION)
                    UpdateLink.Visible = true;
            }
            catch
            {
            }
        }

        private class OptionInfo
        {
            public string Name { get; set; }

            public string NodePath => $"OptionsContainer/CenterContainer/HBoxContainer/{Name}/OptionButton";

            public bool IsAudio { get; set; }

            public Dictionary<string, string[]> IncludeSkinIniProperties { get; set; } = new Dictionary<string, string[]>();

            public string[] IncludeFileNames { get; set; }
        }
    }
}