namespace UniqueIds;

public static class ProjectVars
{
    private const string AuthorString = "Caboose.Sage";
    private const string ModFamilyString = "ItsStardew";
    private const string Header = $"{AuthorString}.{ModFamilyString}";

    public static class Author
    {
        public const string Name = "Caboose Sage";
    }

    public static class ModFamily
    {
        public const string Name = ModFamilyString;
    }
    
    public static class Mods
    {
        private const string Base = $"{Header}.Mod";
        
        public const string MetalsCasting = $"{Base}.MetalsCasting";
    }

    public static class Content
    {
        private const string Base = $"{Header}.Content";

        public static class Packs
        {
            private const string Base = $"{Content.Base}.Pack";
            
            public static class Metals
            {
                private const string Base = $"{Packs.Base}.Metals";
                public const string CastingMachine = $"{Base}.CastingMachine";
                public const string Jewelry = $"{Base}.Jewelry";
            }
        }
        
        public static class Managers
        {
            private const string Base = $"{Content.Base}.Manager";
        
            public const string Metals = $"{Base}.Metals";
        }
    }
}