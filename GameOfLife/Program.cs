#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    public class Program
    {
        public static void Main()
        {
            IGameOfLife gameOfLife = new GameOfLife();
            gameOfLife.Start();
        }
    }

    public static class Rand
    {
        private static readonly Random Random = new Random();

        public static bool RandBool()
        {
            return Random.Next(2) == 1;
        }
    }

    public interface IAnimal
    {
        IEnumerable<IVector2> MovementOrder { get; }
    }

    public interface IMaleAnimal : IAnimal
    {
    }

    public interface IFemaleAnimal : IAnimal
    {
        IAnimal? BreedWith(IMaleAnimal animal);
    }

    public interface IPredatorAnimal : IAnimal
    {
    }

    public interface IEdibleAnimal : IAnimal
    {
        bool CanBeEatenBy(IAnimal animal);
    }

    public static class VectorDirection
    {
        public static readonly IVector2 Up = new Vector2(0, -1);
        public static readonly IVector2 Down = new Vector2(0, 1);
        public static readonly IVector2 Left = new Vector2(-1, 0);
        public static readonly IVector2 Right = new Vector2(1, 0);
    }

    public interface IVector2
    {
        int X { get; }
        int Y { get; }

        IVector2 Add(IVector2 vector);
        IVector2 Add(int x, int y);
    }

    public interface IField
    {
        IVector2 Position { get; }

        IAnimal? OccupyingAnimal { get; set; }
    }

    public interface IBoard
    {
        IEnumerable<IField> GetAllFields();
        IEnumerable<IField> GetAnimalFields();
        IEnumerable<IField> GetFemaleAnimalFields();
        IEnumerable<IField> GetMaleAnimalFields();
        IEnumerable<IField> GetEdibleAnimalFields();

        IField? GetFieldAt(IVector2 position);

        IField? GetFirstRelativeFieldWhere(IVector2 referencePosition, IEnumerable<IVector2> searchOrder,
            Func<IField, bool> predicate);
    }

    public interface ITurnAction
    {
        void Execute(IBoard board);
    }

    public interface IGame
    {
        IBoard Board { get; }
        int CurrentTurnNumber { get; }

        void NextTurn();
    }

    public interface IGameOfLife
    {
        void Start();
    }

    public interface IBoardParser
    {
        IBoard Parse(string[] input);
    }

    public enum AnimalType
    {
        Lion,
        Crocodile,
        Elephant,
        Antelope
    }

    public interface IAnimalLegendKey
    {
        AnimalType Type { get; }
        bool IsMale { get; }
        char Symbol { get; }
    }

    public interface IBoardLegend
    {
        char EmptyFieldSymbol { get; }

        IAnimalLegendKey? GetAnimalLegendKeyFromSymbol(char c);
        IAnimalLegendKey? GetAnimalLegendKey(IAnimal animal);
    }

    public interface IInputReader
    {
        string[] ReadInput();
    }

    public interface IGameDisplay
    {
        void DisplayGame(IGame game);
    }

    public abstract class AnimalBase : IAnimal
    {
        public IEnumerable<IVector2> MovementOrder { get; }

        protected AnimalBase(IEnumerable<IVector2> movementOrder)
        {
            MovementOrder = movementOrder;
        }
    }

    public abstract class Lion : AnimalBase, IPredatorAnimal
    {
        protected Lion() : base(
            new List<IVector2>
            {
                VectorDirection.Right,
                VectorDirection.Left
            }
        )
        {
        }
    }

    public class MaleLion : Lion, IMaleAnimal
    {
    }

    public class FemaleLion : Lion, IFemaleAnimal
    {
        public IAnimal? BreedWith(IMaleAnimal animal)
        {
            if (!(animal is MaleLion))
            {
                return null;
            }

            return AnimalFactory.Create(AnimalType.Lion, Rand.RandBool());
        }
    }

    public abstract class Crocodile : AnimalBase, IPredatorAnimal
    {
        protected Crocodile() : base(
            new List<IVector2>
            {
                VectorDirection.Up,
                VectorDirection.Down
            }
        )
        {
        }
    }

    public class MaleCrocodile : Crocodile, IMaleAnimal
    {
    }

    public class FemaleCrocodile : Crocodile, IFemaleAnimal
    {
        public IAnimal? BreedWith(IMaleAnimal animal)
        {
            if (!(animal is MaleCrocodile))
            {
                return null;
            }

            return AnimalFactory.Create(AnimalType.Crocodile, Rand.RandBool());
        }
    }

    public class Elephant : AnimalBase
    {
        protected Elephant() : base(
            new List<IVector2>
            {
                VectorDirection.Up,
                VectorDirection.Right,
                VectorDirection.Down,
                VectorDirection.Left
            }
        )
        {
        }
    }

    public class MaleElephant : Elephant, IMaleAnimal
    {
    }

    public class FemaleElephant : Elephant, IFemaleAnimal
    {
        public IAnimal? BreedWith(IMaleAnimal animal)
        {
            if (!(animal is MaleElephant))
            {
                return null;
            }

            return AnimalFactory.Create(AnimalType.Elephant, Rand.RandBool());
        }
    }

    public class Antelope : AnimalBase, IEdibleAnimal
    {
        protected Antelope() : base(
            new List<IVector2>
            {
                VectorDirection.Right,
            }
        )
        {
        }

        public bool CanBeEatenBy(IAnimal animal) => animal is IPredatorAnimal;
    }

    public class MaleAntelope : Antelope, IMaleAnimal
    {
    }

    public class FemaleAntelope : Antelope, IFemaleAnimal
    {
        public IAnimal? BreedWith(IMaleAnimal animal)
        {
            if (!(animal is MaleAntelope))
            {
                return null;
            }

            return AnimalFactory.Create(AnimalType.Antelope, Rand.RandBool());
        }
    }


    public static class AnimalFactory
    {
        public static IAnimal Create(AnimalType type, bool isMale)
        {
            if (isMale)
            {
                return type switch
                {
                    AnimalType.Lion => new MaleLion(),
                    AnimalType.Crocodile => new MaleCrocodile(),
                    AnimalType.Elephant => new MaleElephant(),
                    AnimalType.Antelope => new MaleAntelope(),
                    _ => throw new Exception($"Cannot create animal from type: type={type}, isMale={isMale}")
                };
            }

            return type switch
            {
                AnimalType.Lion => new FemaleLion(),
                AnimalType.Crocodile => new FemaleCrocodile(),
                AnimalType.Elephant => new FemaleElephant(),
                AnimalType.Antelope => new FemaleAntelope(),
                _ => throw new Exception($"Cannot create animal from type: type={type}, isMale={isMale}")
            };
        }
    }


    public class Vector2 : IVector2
    {
        public int X { get; }
        public int Y { get; }

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IVector2 Add(IVector2 vector)
        {
            return new Vector2(X + vector.X, Y + vector.Y);
        }

        public IVector2 Add(int x, int y)
        {
            return new Vector2(X + x, Y + y);
        }
    }

    public class Field : IField
    {
        public IVector2 Position { get; }

        public IAnimal? OccupyingAnimal { get; set; }

        public Field(IVector2 position)
        {
            Position = position;
        }
    }

    public class Board : IBoard
    {
        private readonly ICollection<IField> _fields = new List<IField>();

        public Board(int width, int height)
        {
            CreateEmptyFields(width, height);
        }

        private void CreateEmptyFields(int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var position = new Vector2(x, y);
                    var field = new Field(position);
                    _fields.Add(field);
                }
            }
        }

        public IEnumerable<IField> GetAllFields()
        {
            return _fields.ToList();
        }

        public IEnumerable<IField> GetAnimalFields()
        {
            return _fields.Where(field => field.OccupyingAnimal != null).ToList();
        }

        public IEnumerable<IField> GetFemaleAnimalFields()
        {
            return _fields.Where(field => field.OccupyingAnimal is IFemaleAnimal).ToList();
        }

        public IEnumerable<IField> GetMaleAnimalFields()
        {
            return _fields.Where(field => field.OccupyingAnimal is IMaleAnimal).ToList();
        }

        public IEnumerable<IField> GetEdibleAnimalFields()
        {
            return _fields.Where(field => field.OccupyingAnimal is IEdibleAnimal).ToList();
        }

        public IField? GetFirstRelativeFieldWhere(IVector2 referencePosition, IEnumerable<IVector2> searchOrder,
            Func<IField, bool> predicate)
        {
            foreach (var vector in searchOrder)
            {
                var checkPosition = referencePosition.Add(vector);
                var checkField = GetFieldAt(checkPosition);
                if (checkField != null && predicate(checkField))
                {
                    return checkField;
                }
            }

            return null;
        }

        public IField? GetFieldAt(IVector2 position)
        {
            return _fields.FirstOrDefault(field =>
                field.Position.X == position.X &&
                field.Position.Y == position.Y
            );
        }
    }

    public class MovementTurnAction : ITurnAction
    {
        public void Execute(IBoard board)
        {
            var animalFields = board.GetAnimalFields()
                .OrderBy(field => field.Position.Y)
                .ThenBy(field => field.Position.X);

            foreach (var animalField in animalFields)
            {
                var animal = animalField.OccupyingAnimal!;
                var destinationField = board.GetFirstRelativeFieldWhere(animalField.Position, animal.MovementOrder,
                    field => field.OccupyingAnimal == null);

                if (destinationField == null)
                {
                    continue;
                }

                animalField.OccupyingAnimal = null;
                destinationField.OccupyingAnimal = animal;
            }
        }
    }

    public class EatingTurnAction : ITurnAction
    {
        private static readonly ICollection<IVector2> EatingDirections = new List<IVector2>
        {
            VectorDirection.Up,
            VectorDirection.Right,
            VectorDirection.Down,
            VectorDirection.Left
        };

        public void Execute(IBoard board)
        {
            var edibleAnimalFields = board.GetEdibleAnimalFields();

            foreach (var edibleAnimalField in edibleAnimalFields)
            {
                var edibleAnimal = (IEdibleAnimal) edibleAnimalField.OccupyingAnimal!;
                var predatorField = board.GetFirstRelativeFieldWhere(edibleAnimalField.Position, EatingDirections,
                    field => field.OccupyingAnimal != null && edibleAnimal.CanBeEatenBy(field.OccupyingAnimal!));

                if (predatorField == null)
                {
                    continue;
                }

                edibleAnimalField.OccupyingAnimal = null;
            }
        }
    }

    public class BreedingTurnAction : ITurnAction
    {
        private static readonly ICollection<IVector2> BreedingDirections = new List<IVector2>
        {
            VectorDirection.Up,
            VectorDirection.Down,
            VectorDirection.Left,
            VectorDirection.Right
        };

        private static readonly ICollection<IVector2> MalePriority = new List<IVector2>
        {
            VectorDirection.Up,
            VectorDirection.Right,
            VectorDirection.Left,
            VectorDirection.Down
        };

        public void Execute(IBoard board)
        {
            var femaleAnimalFields = board.GetFemaleAnimalFields();
            var maleAnimalFields = board.GetMaleAnimalFields();

            foreach (var femaleAnimalField in femaleAnimalFields)
            {
                var femaleAnimal = (IFemaleAnimal) femaleAnimalField.OccupyingAnimal!;
                foreach (var maleDirection in MalePriority)
                {
                    var maleCheckPosition = femaleAnimalField.Position.Add(maleDirection);
                    var maleField = maleAnimalFields.FirstOrDefault(field =>
                        field.Position.X == maleCheckPosition.X &&
                        field.Position.Y == maleCheckPosition.Y
                    );

                    if (maleField == null)
                    {
                        continue;
                    }

                    var maleAnimal = (IMaleAnimal) maleField.OccupyingAnimal!;
                    var child = femaleAnimal.BreedWith(maleAnimal);
                    if (child == null)
                    {
                        continue;
                    }

                    var childField = board.GetFirstRelativeFieldWhere(femaleAnimalField.Position, BreedingDirections,
                        field => field.OccupyingAnimal == null);

                    if (childField == null)
                    {
                        continue;
                    }

                    childField.OccupyingAnimal = child;
                }
            }
        }
    }

    public class Game : IGame
    {
        private static readonly IEnumerable<ITurnAction> GameTurnActions = new List<ITurnAction>
        {
            new MovementTurnAction(),
            new EatingTurnAction(),
            new BreedingTurnAction()
        };

        public IBoard Board { get; }
        public int CurrentTurnNumber { get; private set; } = 0;

        private IEnumerable<ITurnAction> TurnActions => GameTurnActions;

        public Game(IBoard board)
        {
            Board = board;
        }

        public void NextTurn()
        {
            foreach (var action in TurnActions)
            {
                action.Execute(Board);
            }

            CurrentTurnNumber++;
        }
    }

    public class BoardParser : IBoardParser
    {
        private readonly IBoardLegend _boardLegend;

        public BoardParser(IBoardLegend boardLegend)
        {
            _boardLegend = boardLegend;
        }

        public IBoard Parse(string[] input)
        {
            if (input.Length == 0)
            {
                throw new ArgumentException("Input does not contain a single row.");
            }

            var width = input.First().Length;
            if (input.Any(row => row.Length != width))
            {
                throw new ArgumentException("Input contains row with width other than the first one.");
            }

            var height = input.Length;
            var board = new Board(width, height);

            IVector2 currentPosition = new Vector2(-1, 0);
            foreach (var row in input)
            {
                foreach (var symbol in row.ToCharArray())
                {
                    currentPosition = currentPosition.Add(1, 0);
                    if (symbol == _boardLegend.EmptyFieldSymbol)
                    {
                        continue;
                    }

                    var animalLegendKey = _boardLegend.GetAnimalLegendKeyFromSymbol(symbol);
                    if (animalLegendKey == null)
                    {
                        throw new Exception($"Unknown symbol '{symbol}'.");
                    }

                    var animal = AnimalFactory.Create(animalLegendKey.Type, animalLegendKey.IsMale);
                    var field = board.GetFieldAt(currentPosition)!;
                    field.OccupyingAnimal = animal;
                }

                currentPosition = new Vector2(-1, currentPosition.Y + 1);
            }

            return board;
        }
    }

    public class AnimalLegendKey : IAnimalLegendKey
    {
        public AnimalType Type { get; }
        public bool IsMale { get; }
        public char Symbol { get; }

        public AnimalLegendKey(AnimalType type, bool isMale, char symbol)
        {
            Type = type;
            IsMale = isMale;
            Symbol = symbol;
        }
    }

    public class BoardLegend : IBoardLegend
    {
        private const char EmptyFieldSymbolChar = '.';

        private static readonly IEnumerable<IAnimalLegendKey> AnimalLegendKeys = new List<IAnimalLegendKey>
        {
            new AnimalLegendKey(AnimalType.Lion, true, 'L'),
            new AnimalLegendKey(AnimalType.Lion, false, 'l'),
            new AnimalLegendKey(AnimalType.Crocodile, true, 'K'),
            new AnimalLegendKey(AnimalType.Crocodile, false, 'k'),
            new AnimalLegendKey(AnimalType.Elephant, true, 'S'),
            new AnimalLegendKey(AnimalType.Elephant, false, 's'),
            new AnimalLegendKey(AnimalType.Antelope, true, 'A'),
            new AnimalLegendKey(AnimalType.Antelope, false, 'a'),
        };

        public char EmptyFieldSymbol => EmptyFieldSymbolChar;

        public IAnimalLegendKey? GetAnimalLegendKeyFromSymbol(char c)
        {
            return AnimalLegendKeys.FirstOrDefault(key => key.Symbol == c);
        }

        public IAnimalLegendKey? GetAnimalLegendKey(IAnimal animal)
        {
            var type = MatchAnimalType(animal);
            if (type == null)
            {
                return null;
            }

            return AnimalLegendKeys.FirstOrDefault(key => key.Type == type && key.IsMale == animal is IMaleAnimal);
        }

        private AnimalType? MatchAnimalType(IAnimal animal)
        {
            return animal switch
            {
                Lion _ => AnimalType.Lion,
                Crocodile _ => AnimalType.Crocodile,
                Elephant _ => AnimalType.Elephant,
                Antelope _ => AnimalType.Antelope,
                _ => null
            };
        }
    }

    public class ConstInputReader : IInputReader
    {
        private readonly string[] _input;

        public ConstInputReader(string[] input)
        {
            _input = input;
        }

        public string[] ReadInput()
        {
            return _input;
        }
    }

    public class ConsoleGameDisplay : IGameDisplay
    {
        private readonly IBoardLegend _boardLegend;

        public ConsoleGameDisplay(IBoardLegend boardLegend)
        {
            _boardLegend = boardLegend;
        }

        public void DisplayGame(IGame game)
        {
            var displayBuilder = new StringBuilder();
            displayBuilder.AppendLine($"Turn {game.CurrentTurnNumber}");

            var fieldGroupedByRow = game.Board
                .GetAllFields()
                .OrderBy(field => field.Position.X)
                .GroupBy(field => field.Position.Y);

            foreach (var grouping in fieldGroupedByRow)
            {
                foreach (var field in grouping)
                {
                    var symbol = _boardLegend.EmptyFieldSymbol;
                    if (field.OccupyingAnimal != null)
                    {
                        var animal = field.OccupyingAnimal!;
                        symbol = _boardLegend
                            .GetAnimalLegendKey(animal)?
                            .Symbol ?? throw new Exception($"There is no legend key defined for {animal}");
                    }

                    displayBuilder.Append(symbol);
                }

                displayBuilder.AppendLine();
            }

            Console.Write(displayBuilder);
        }
    }

    public class GameOfLife : IGameOfLife
    {
        public void Start()
        {
            IInputReader inputReader = new ConstInputReader(
                new[]
                {
                    ".sa..Aa.....S...",
                    "L..K..........L.",
                    "......S...s.....",
                    "l..k............",
                    "......S.....A...",
                    "........a.......",
                    "..l.............",
                    "l.....K........."
                }
            );

            IBoardLegend boardLegend = new BoardLegend();
            IBoardParser boardParser = new BoardParser(boardLegend);
            IGameDisplay gameDisplay = new ConsoleGameDisplay(boardLegend);

            var input = inputReader.ReadInput();
            IBoard board = boardParser.Parse(input);
            IGame game = new Game(board);

            gameDisplay.DisplayGame(game);
            for (var turn = 1; turn <= 10; turn++)
            {
                game.NextTurn();
                gameDisplay.DisplayGame(game);
            }
        }
    }
}