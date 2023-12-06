using System.Text.RegularExpressions;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day04;

public class Scratchcards : BaseDayModule
{
    public Scratchcards(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 4;
    public override string Title => "Scratchcards";

    [Fact] public void Part1_Sample() => Part1_CardsTotalPoints(GetData(InputType.Sample)).Should().Be(13);
    [Fact] public void Part1() => Part1_CardsTotalPoints(GetData(InputType.Input));

    [Fact] public void Part2_Sample() => Part2_CardCopies(GetData(InputType.Sample)).Should().Be(30);
    [Fact] public void Part2() => Part2_CardCopies(GetData(InputType.Input));
    
    public int Part1_CardsTotalPoints(string data)
    {
        var cards = data.ToLines().Select(ParseCard).ToList();
        WriteLine($"Loaded data with {cards.Count} cards.");
        
        var cardsTotalPoints = cards.Sum(x => x.PointValue());
        WriteLine($"Total Points: {cardsTotalPoints}");
        return cardsTotalPoints;
    }
    
    public int Part2_CardCopies(string data)
    {
        var cards = data.ToLines().Select(ParseCard).ToList();
        WriteLine($"Loaded data with {cards.Count} cards.");

        // set up a way to look up cards by id for easier copying
        var originalCardsById = cards.ToDictionary(x => x.Id, x => x);
        
        // start with a base collection of all original cards, then process them and add to this list as we go
        var cardOutput = new List<ScratchCard>();
        cardOutput.AddRange(cards);

        for (int i = 0; i < cardOutput.Count; i++)
        {
            var currentCard = cardOutput[i];

            var copiesWon = currentCard.MatchingNumbers().Count;
            if (copiesWon > 0)
            {
                // figure out the ids of the cards to copy
                var idsToCopy = Enumerable.Range(currentCard.Id + 1, copiesWon).ToList();
                // then add them to the end of the output list, they'll be processed when we get to them
                idsToCopy.ForEach(copyId => cardOutput.Add(originalCardsById[copyId]));
            }
        }

        var totalCards = cardOutput.Count;
        WriteLine($"Total Cards after processing/copying: {totalCards}");
        return totalCards;
    }
    
    private ScratchCard ParseCard(string line)
    {
        var cardIdRegEx = new Regex(@"^Card\s+(?<Id>\d+):", RegexOptions.None);
        var cardId = int.Parse(cardIdRegEx.Match(line).Groups["Id"].Value);
        
        var winningNumbers = line.Split(":")[1].Split("|")[0]
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse).ToList();
        
        var scratchNumbers = line.Split(":")[1].Split("|")[1]
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse).ToList();

        var card = new ScratchCard(cardId, winningNumbers, scratchNumbers);
        
        return card;
    }
    
    private record ScratchCard(int Id, List<int> WinningNumbers, List<int> ScratchNumbers)
    {
        public List<int> MatchingNumbers() => ScratchNumbers.Intersect(WinningNumbers).ToList();

        /// <summary>
        /// Calculates point value of the card used in Part 1
        /// </summary>
        public int PointValue()
        {
            var totalMatchingNumbers = MatchingNumbers().Count;
            var points = totalMatchingNumbers == 0 ? 0 : (int)Math.Pow(2, (totalMatchingNumbers - 1));
            return points;
        }
    }
}

