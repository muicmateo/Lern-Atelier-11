using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IGeminiService _geminiService;
    private readonly Random _random = new();

    [ObservableProperty]
    private string characterName = string.Empty;

    [ObservableProperty]
    private string selectedClass = string.Empty;

    [ObservableProperty]
    private string selectedRace = string.Empty;

    [ObservableProperty]
    private string selectedGender = string.Empty;

    [ObservableProperty]
    private int selectedLevel = 1;

    [ObservableProperty]
    private string selectedBackground = string.Empty;

    [ObservableProperty]
    private string selectedSkill1 = string.Empty;

    [ObservableProperty]
    private string selectedSkill2 = string.Empty;

    [ObservableProperty]
    private string selectedSkill3 = string.Empty;

    [ObservableProperty]
    private string selectedTalent = string.Empty;

    [ObservableProperty]
    private string prompt = string.Empty;

    [ObservableProperty]
    private string generatedStory = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isStoryGenerated;

    // Character Attributes
    [ObservableProperty]
    private int strength = 10;

    [ObservableProperty]
    private int dexterity = 10;

    [ObservableProperty]
    private int constitution = 10;

    [ObservableProperty]
    private int intelligence = 10;

    [ObservableProperty]
    private int wisdom = 10;

    [ObservableProperty]
    private int charisma = 10;

    // Attribute Generation Method
    [ObservableProperty]
    private string selectedAttributeMethod = "Point Buy";

    [ObservableProperty]
    private int remainingPoints = 27;

    public ObservableCollection<string> Classes { get; }
    public ObservableCollection<string> Races { get; }
    public ObservableCollection<string> Genders { get; }
    public ObservableCollection<int> Levels { get; }
    public ObservableCollection<string> Backgrounds { get; }
    public ObservableCollection<string> Skills { get; }
    public ObservableCollection<string> Talents { get; }
    public ObservableCollection<string> AttributeMethods { get; }

    public bool IsNotBusy => !IsBusy;
    public bool IsPointBuyMode => SelectedAttributeMethod == "Point Buy";
    public bool IsDiceRollMode => SelectedAttributeMethod == "Dice Roll";

    public MainViewModel(IGeminiService geminiService)
    {
        _geminiService = geminiService;
        
        Classes = new ObservableCollection<string>
        {
            "Fighter", "Wizard", "Rogue", "Cleric", "Ranger", "Barbarian", "Bard", "Druid", "Monk", "Paladin", "Sorcerer", "Warlock"
        };
        
        Races = new ObservableCollection<string>
        {
            "Human", "Elf", "Dwarf", "Halfling", "Dragonborn", "Gnome", "Half-Elf", "Half-Orc", "Tiefling"
        };

        Genders = new ObservableCollection<string>
        {
            "Male", "Female", "Non-binary", "Other", "Prefer not to specify"
        };

        Levels = new ObservableCollection<int>();
        for (int i = 1; i <= 20; i++)
        {
            Levels.Add(i);
        }

        Backgrounds = new ObservableCollection<string>
        {
            "Acolyte", "Criminal", "Folk Hero", "Noble", "Sage", "Soldier", "Charlatan", "Entertainer", 
            "Guild Artisan", "Hermit", "Outlander", "Sailor", "Urchin", "Haunted One", "Knight", 
            "Pirate", "Spy", "Tribal Warrior", "Merchant", "Scholar"
        };

        Skills = new ObservableCollection<string>
        {
            "Acrobatics", "Animal Handling", "Arcana", "Athletics", "Deception", "History", "Insight", 
            "Intimidation", "Investigation", "Medicine", "Nature", "Perception", "Performance", 
            "Persuasion", "Religion", "Sleight of Hand", "Stealth", "Survival"
        };

        Talents = new ObservableCollection<string>
        {
            "Alert", "Actor", "Athlete", "Charger", "Crossbow Expert", "Defensive Duelist", "Dual Wielder",
            "Dungeon Delver", "Durable", "Elemental Adept", "Fey Touched", "Great Weapon Master", 
            "Healer", "Heavy Armor Master", "Inspiring Leader", "Keen Mind", "Lucky", "Magic Initiate",
            "Martial Adept", "Mobile", "Moderately Armored", "Mounted Combatant", "Observant", 
            "Polearm Master", "Resilient", "Ritual Caster", "Savage Attacker", "Sentinel", 
            "Shadow Touched", "Sharpshooter", "Shield Master", "Skilled", "Skulker", "Spell Sniper",
            "Tavern Brawler", "Telekinetic", "Telepathic", "Tough", "War Caster", "Weapon Master"
        };

        AttributeMethods = new ObservableCollection<string>
        {
            "Point Buy", "Dice Roll"
        };

        // Set defaults
        SelectedLevel = 1;
        SelectedAttributeMethod = "Point Buy";
        ResetAttributesToBase();
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    partial void OnSelectedAttributeMethodChanged(string value)
    {
        OnPropertyChanged(nameof(IsPointBuyMode));
        OnPropertyChanged(nameof(IsDiceRollMode));
        
        if (value == "Point Buy")
        {
            ResetAttributesToBase();
        }
    }

    [RelayCommand]
    private void RollAllAttributes()
    {
        if (IsDiceRollMode)
        {
            Strength = RollAttribute();
            Dexterity = RollAttribute();
            Constitution = RollAttribute();
            Intelligence = RollAttribute();
            Wisdom = RollAttribute();
            Charisma = RollAttribute();
        }
    }

    [RelayCommand]
    private void RollSingleAttribute(string attributeName)
    {
        if (IsDiceRollMode)
        {
            var newValue = RollAttribute();
            switch (attributeName?.ToLower())
            {
                case "strength":
                    Strength = newValue;
                    break;
                case "dexterity":
                    Dexterity = newValue;
                    break;
                case "constitution":
                    Constitution = newValue;
                    break;
                case "intelligence":
                    Intelligence = newValue;
                    break;
                case "wisdom":
                    Wisdom = newValue;
                    break;
                case "charisma":
                    Charisma = newValue;
                    break;
            }
        }
    }

    [RelayCommand]
    private void IncreaseAttribute(string attributeName)
    {
        if (!IsPointBuyMode) return;

        var currentValue = GetAttributeValue(attributeName);
        var cost = GetAttributeCost(currentValue);
        
        if (currentValue < 15 && RemainingPoints >= cost)
        {
            SetAttributeValue(attributeName, currentValue + 1);
            RemainingPoints -= cost;
        }
    }

    [RelayCommand]
    private void DecreaseAttribute(string attributeName)
    {
        if (!IsPointBuyMode) return;

        var currentValue = GetAttributeValue(attributeName);
        
        if (currentValue > 8)
        {
            var costRefund = GetAttributeCost(currentValue - 1);
            SetAttributeValue(attributeName, currentValue - 1);
            RemainingPoints += costRefund;
        }
    }

    private int RollAttribute()
    {
        // Roll 4d6, drop lowest (standard D&D method)
        var rolls = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            rolls.Add(_random.Next(1, 7));
        }
        rolls.Sort();
        return rolls.Skip(1).Sum(); // Drop the lowest roll
    }

    private int GetAttributeCost(int value)
    {
        return value switch
        {
            >= 14 => 2, // 14->15 costs 2 points
            >= 13 => 1, // 13->14 costs 1 point
            _ => 1      // 8->13 costs 1 point each
        };
    }

    private int GetAttributeValue(string attributeName)
    {
        return attributeName?.ToLower() switch
        {
            "strength" => Strength,
            "dexterity" => Dexterity,
            "constitution" => Constitution,
            "intelligence" => Intelligence,
            "wisdom" => Wisdom,
            "charisma" => Charisma,
            _ => 10
        };
    }

    private void SetAttributeValue(string attributeName, int value)
    {
        switch (attributeName?.ToLower())
        {
            case "strength":
                Strength = value;
                break;
            case "dexterity":
                Dexterity = value;
                break;
            case "constitution":
                Constitution = value;
                break;
            case "intelligence":
                Intelligence = value;
                break;
            case "wisdom":
                Wisdom = value;
                break;
            case "charisma":
                Charisma = value;
                break;
        }
    }

    private void ResetAttributesToBase()
    {
        Strength = 8;
        Dexterity = 8;
        Constitution = 8;
        Intelligence = 8;
        Wisdom = 8;
        Charisma = 8;
        RemainingPoints = 27;
    }

    private int GetAttributeModifier(int attributeValue)
    {
        return (attributeValue - 10) / 2;
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsStoryGenerated = false;

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Generate a detailed Dungeons and Dragons character backstory with the following characteristics:");
            promptBuilder.AppendLine($"Name: {CharacterName}");
            promptBuilder.AppendLine($"Race: {SelectedRace}");
            promptBuilder.AppendLine($"Class: {SelectedClass}");
            promptBuilder.AppendLine($"Level: {SelectedLevel}");
            
            if (!string.IsNullOrWhiteSpace(SelectedGender))
                promptBuilder.AppendLine($"Gender: {SelectedGender}");
            
            if (!string.IsNullOrWhiteSpace(SelectedBackground))
                promptBuilder.AppendLine($"Background: {SelectedBackground}");

            // Add attributes to the prompt
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Character Attributes:");
            promptBuilder.AppendLine($"Strength: {Strength} (modifier: {GetAttributeModifier(Strength):+0;-#})");
            promptBuilder.AppendLine($"Dexterity: {Dexterity} (modifier: {GetAttributeModifier(Dexterity):+0;-#})");
            promptBuilder.AppendLine($"Constitution: {Constitution} (modifier: {GetAttributeModifier(Constitution):+0;-#})");
            promptBuilder.AppendLine($"Intelligence: {Intelligence} (modifier: {GetAttributeModifier(Intelligence):+0;-#})");
            promptBuilder.AppendLine($"Wisdom: {Wisdom} (modifier: {GetAttributeModifier(Wisdom):+0;-#})");
            promptBuilder.AppendLine($"Charisma: {Charisma} (modifier: {GetAttributeModifier(Charisma):+0;-#})");
            
            if (!string.IsNullOrWhiteSpace(SelectedSkill1) || !string.IsNullOrWhiteSpace(SelectedSkill2) || !string.IsNullOrWhiteSpace(SelectedSkill3))
            {
                promptBuilder.Append("Key Skills: ");
                var skills = new[] { SelectedSkill1, SelectedSkill2, SelectedSkill3 }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
                promptBuilder.AppendLine(string.Join(", ", skills));
            }
            
            if (!string.IsNullOrWhiteSpace(SelectedTalent))
                promptBuilder.AppendLine($"Special Talent/Feat: {SelectedTalent}");
            
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Please create a rich backstory that incorporates these elements and explains how they developed these skills and abilities. Consider their experience level ({GetLevelDescription(SelectedLevel)}) and how their attributes reflect their personality and capabilities.");
            
            if (!string.IsNullOrWhiteSpace(Prompt))
            {
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Additional requirements:");
                promptBuilder.AppendLine(Prompt);
            }

            GeneratedStory = await _geminiService.GetCompletionAsync(promptBuilder.ToString());
            IsStoryGenerated = !string.IsNullOrWhiteSpace(GeneratedStory);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string GetLevelDescription(int level)
    {
        return level switch
        {
            1 => "a novice adventurer just starting their journey",
            >= 2 and <= 4 => "an apprentice with some basic experience",
            >= 5 and <= 10 => "an experienced adventurer with proven skills",
            >= 11 and <= 16 => "a veteran hero with significant accomplishments",
            >= 17 and <= 20 => "a legendary figure with extraordinary achievements",
            _ => "an adventurer"
        };
    }

    [RelayCommand]
    private async Task ExportCharacter()
    {
        if (!IsStoryGenerated || string.IsNullOrWhiteSpace(GeneratedStory))
        {
            await Application.Current.MainPage.DisplayAlert("Export Error", "Please generate a character backstory first.", "OK");
            return;
        }

        try
        {
            var characterSheet = GenerateCharacterSheet();
            var fileName = $"{CharacterName.Replace(" ", "_")}_Character_Sheet.txt";
            
            await SaveCharacterSheet(characterSheet, fileName);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Export Error", $"Failed to export character: {ex.Message}", "OK");
        }
    }

    private string GenerateCharacterSheet()
    {
        var sheet = new StringBuilder();
        sheet.AppendLine("???????????????????????????????????????????????");
        sheet.AppendLine("            D&D CHARACTER SHEET");
        sheet.AppendLine("???????????????????????????????????????????????");
        sheet.AppendLine();
        sheet.AppendLine($"CHARACTER NAME: {CharacterName}");
        sheet.AppendLine($"RACE: {SelectedRace}");
        sheet.AppendLine($"CLASS: {SelectedClass}");
        sheet.AppendLine($"LEVEL: {SelectedLevel}");
        
        if (!string.IsNullOrWhiteSpace(SelectedGender))
            sheet.AppendLine($"GENDER: {SelectedGender}");
        
        if (!string.IsNullOrWhiteSpace(SelectedBackground))
            sheet.AppendLine($"BACKGROUND: {SelectedBackground}");

        // Add attributes section
        sheet.AppendLine();
        sheet.AppendLine("ATTRIBUTES:");
        sheet.AppendLine($"  Strength:     {Strength,2} ({GetAttributeModifier(Strength):+0;-#})");
        sheet.AppendLine($"  Dexterity:    {Dexterity,2} ({GetAttributeModifier(Dexterity):+0;-#})");
        sheet.AppendLine($"  Constitution: {Constitution,2} ({GetAttributeModifier(Constitution):+0;-#})");
        sheet.AppendLine($"  Intelligence: {Intelligence,2} ({GetAttributeModifier(Intelligence):+0;-#})");
        sheet.AppendLine($"  Wisdom:       {Wisdom,2} ({GetAttributeModifier(Wisdom):+0;-#})");
        sheet.AppendLine($"  Charisma:     {Charisma,2} ({GetAttributeModifier(Charisma):+0;-#})");
        
        if (!string.IsNullOrWhiteSpace(SelectedSkill1) || !string.IsNullOrWhiteSpace(SelectedSkill2) || !string.IsNullOrWhiteSpace(SelectedSkill3))
        {
            sheet.AppendLine();
            sheet.AppendLine("KEY SKILLS:");
            if (!string.IsNullOrWhiteSpace(SelectedSkill1)) sheet.AppendLine($"  • {SelectedSkill1}");
            if (!string.IsNullOrWhiteSpace(SelectedSkill2)) sheet.AppendLine($"  • {SelectedSkill2}");
            if (!string.IsNullOrWhiteSpace(SelectedSkill3)) sheet.AppendLine($"  • {SelectedSkill3}");
        }
        
        if (!string.IsNullOrWhiteSpace(SelectedTalent))
        {
            sheet.AppendLine();
            sheet.AppendLine($"SPECIAL TALENT: {SelectedTalent}");
        }
        
        sheet.AppendLine();
        sheet.AppendLine("???????????????????????????????????????????????");
        sheet.AppendLine("                 BACKSTORY");
        sheet.AppendLine("???????????????????????????????????????????????");
        sheet.AppendLine();
        sheet.AppendLine(GeneratedStory);
        sheet.AppendLine();
        sheet.AppendLine("???????????????????????????????????????????????");
        sheet.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sheet.AppendLine($"Method: {SelectedAttributeMethod}");
        sheet.AppendLine("Created with D&D Character Creator");
        sheet.AppendLine("???????????????????????????????????????????????");
        
        return sheet.ToString();
    }

    private async Task SaveCharacterSheet(string content, string fileName)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = content,
            Title = "Export Character Sheet",
            Subject = $"D&D Character: {CharacterName}"
        });
    }
}