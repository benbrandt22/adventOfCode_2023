using System.Diagnostics;
using System.Text.RegularExpressions;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day02;

public class CubeConundrum : BaseDayModule
{
    public CubeConundrum(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 2;
    public override string Title => "Cube Conundrum";

    [Fact] public void Part1_Sample() => Part1_TotalOfPossibleGameIds(GetData(InputType.Sample)).Should().Be(8);
    [Fact] public void Part1() => Part1_TotalOfPossibleGameIds(GetData(InputType.Input));
    
    [Fact] public void Part2_Sample() => Part2_TotalOfMinimumBagPowers(GetData(InputType.Sample)).Should().Be(2286);
    [Fact] public void Part2() => Part2_TotalOfMinimumBagPowers(GetData(InputType.Input));
    
    public int Part1_TotalOfPossibleGameIds(string data)
    {
        var games = data.ToLines().Select(ParseGame).ToList();
        WriteLine($"Loaded data with {games.Count} games.");

        var theoreticalBag = new CubeGameBag(new List<CubeColorQuantity>()
        {
            new(12, CubeColor.Red),
            new(13, CubeColor.Green),
            new(14, CubeColor.Blue)
        });
        
        var possibleGames = games.Where(game =>
        {
            var possible = game.Rounds.All(round => theoreticalBag.CouldContain(round.CubeColorQuantities));
            return possible;
        }).ToList();

        WriteLine($"{possibleGames.Count} possible games.");
        var sumOfGameIds = possibleGames.Sum(g => g.GameId);
        WriteLine($"Sum of game ids: {sumOfGameIds}");
        
        return sumOfGameIds;
    }
    
    public int Part2_TotalOfMinimumBagPowers(string data)
    {
        var games = data.ToLines().Select(ParseGame).ToList();
        WriteLine($"Loaded data with {games.Count} games.");

        var totalMinimumBagPowers = games
            .Select(CalculateMinimumViableBag)
            .Select(bag => bag.CalculatePower())
            .Sum();

        WriteLine($"Total minimum bag powers: {totalMinimumBagPowers}");
        
        return totalMinimumBagPowers;
    }

    private CubeGameBag CalculateMinimumViableBag(CubeGame game)
    {
        var allQuantities = game.Rounds.SelectMany(r => r.CubeColorQuantities).ToList();
        var minimumNecessary = new[]{ CubeColor.Red, CubeColor.Green, CubeColor.Blue }
            .Select(color =>
            {
                var biggestColorGrab = allQuantities.Where(q => q.Color == color).Max(q => q.Quantity);
                return new CubeColorQuantity(biggestColorGrab, color);
            }).ToList();
        return new CubeGameBag(minimumNecessary);
    }


    [DebuggerDisplay("Game {GameId}, {Rounds.Count} rounds")]
    private class CubeGame
    {
        public int GameId { get; set; }
        public List<CubeGameRound> Rounds { get; set; } = new();
    }

    private class CubeGameRound(List<CubeColorQuantity> cubeColorQuantities)
    {
        public List<CubeColorQuantity> CubeColorQuantities => cubeColorQuantities;
    }
    
    private class CubeColorQuantity(int quantity, CubeColor color)
    {
        public int Quantity => quantity;
        public CubeColor Color => color;
    }

    private enum CubeColor { Red, Green, Blue }

    private class CubeGameBag(List<CubeColorQuantity> cubeColorQuantities)
    {
        public bool CouldContain(List<CubeColorQuantity> otherCubeColorQuantities)
        {
            bool possible = true;
            foreach (var other in otherCubeColorQuantities)
            {
                possible = possible && cubeColorQuantities.Any(x => x.Color == other.Color && x.Quantity >= other.Quantity);
            }

            return possible;
        }

        public int CalculatePower()
        {
            var redValue = cubeColorQuantities.Where(x => x.Color == CubeColor.Red).Sum(x => x.Quantity);
            var greenValue = cubeColorQuantities.Where(x => x.Color == CubeColor.Green).Sum(x => x.Quantity);
            var blueValue = cubeColorQuantities.Where(x => x.Color == CubeColor.Blue).Sum(x => x.Quantity);
            var power = redValue * greenValue * blueValue;
            return power;
        }
    }

    /// <summary>
    /// Takes the input text and parses it into a CubeGame object.
    /// </summary>
    private CubeGame ParseGame(string line)
    {
        var gameIdRegEx = new Regex(@"^Game (?<gameId>\d+):", RegexOptions.None);
        var roundCubeQtyRegEx = new Regex(@"(?<quantity>\d+) (?<color>red|green|blue)", RegexOptions.None);
        
        var game = gameIdRegEx.Match(line).MapTo<CubeGame>();

        var rounds = line.Split(":").Last().Split(";");
        
        foreach (var roundText in rounds)
        {
            var cubeColorQuantities = roundCubeQtyRegEx.Matches(roundText).MapEachTo<CubeColorQuantity>();
            var round = new CubeGameRound(cubeColorQuantities.ToList());
            game.Rounds.Add(round);
        }
        
        return game;
    }
}

