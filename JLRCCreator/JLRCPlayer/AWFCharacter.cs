using System.Collections.Generic;

//https://stackoverflow.com/questions/72629354/how-to-use-multiple-controls-to-display-each-character-in-a-string-in-wpf
namespace AWFPlayer
{
    public struct AWFCharacter
    {
        public char Character { get; }
        public string Annotation { get; }
        public AWFCharacter(char character, string annotation)
        {
            this.Character = character;
            this.Annotation = annotation;
        }
    }

    public struct AWFCharacterLine
    {
        public IList<AWFCharacter> AWFCharacters { get; }
        public static AWFCharacterLine Blank => new AWFCharacterLine(new List<AWFCharacter>() { new AWFCharacter(' ', string.Empty) });
        public AWFCharacterLine(IList<AWFCharacter> awfCharacters)
        {
            this.AWFCharacters = awfCharacters;
        }
    }
}
