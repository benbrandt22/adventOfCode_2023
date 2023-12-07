using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day07;

public class CamelCards : BaseDayModule
{
    public CamelCards(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 7;
    public override string Title => "Camel Cards";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(6440);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(5905);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var handsAndBids = data.ToLines(true).Select(ParseHandAndBid).ToList();
        WriteLine($"Part 1 - Loaded {handsAndBids.Count} hands and bids");
        return ProcessHandsAndBids(handsAndBids);
    }

    public long ExecutePart2(string data)
    {
        var handsAndBids = data.Replace("J","*")
            .ToLines(true).Select(ParseHandAndBid).ToList();
        WriteLine($"Part 2 - Loaded {handsAndBids.Count} hands and bids, Changing \"J\" to \"*\" to treat as Jokers");
        return ProcessHandsAndBids(handsAndBids);
    }

    private long ProcessHandsAndBids(List<HandAndBid> handsAndBids)
    {
        var totalHands = handsAndBids.Count;
        
        var rankedHandsAndBids = handsAndBids
            .OrderByDescending(h => h.Hand)
            .Select((hb, i) => new{ Hand = hb.Hand, Bid = hb.Bid, Rank = (totalHands-i) })
            .ToList();
        
        rankedHandsAndBids.ForEach(x => WriteLine($" {x.Hand} Bid: {x.Bid} Rank: {x.Rank}"));

        var totalWinnings = rankedHandsAndBids.Sum(hb => (hb.Bid * hb.Rank));
        WriteLine($"Total Winnings: {totalWinnings}");
        return totalWinnings;
    }

    private HandAndBid ParseHandAndBid(string line)
    {
        var handString = line.Split(' ').First().Trim();
        var bid = long.Parse(line.Split(' ').Last());
        return new HandAndBid(new Hand(handString), bid);
    }
    
    public record Card(char Display, long Strength);

    public class Hand : IComparable<Hand>
    {
        public List<Card> Cards { get; }
        public HandType HandType { get; }

        public Hand(string handString) : this(handString.Select(c => CardLookup[c]).ToList())
        {
        }

        public Hand(List<Card> cards)
        {
            if(cards.Count != 5) throw new ArgumentException("A hand must contain 5 cards");
            Cards = cards;
            HandType = CalculateHandType();
        }

        public override string ToString()
        {
            return $"{new string(Cards.Select(c => c.Display).ToArray())} ({HandType})";
        }

        private HandType CalculateHandType()
        {
            if (Cards.Any(c => c.Display == '*')) return FindBestJokerHandType(Cards);
            
            var cardGroups = Cards.GroupBy(c => c.Display).ToList();
            var cardGroupCounts = cardGroups.Select(g => g.Count()).ToList();
            
            if (cardGroupCounts.Contains(5)) { return HandType.FiveOfAKind; }
            else if (cardGroupCounts.Contains(4)) { return HandType.FourOfAKind; }
            else if (cardGroupCounts.Contains(3) && cardGroupCounts.Contains(2)) { return HandType.FullHouse; }
            else if (cardGroupCounts.Contains(3)) { return HandType.ThreeOfAKind; }
            else if (cardGroupCounts.Contains(2) && cardGroupCounts.Count(c => c == 2) == 2) { return HandType.TwoPair; }
            else if (cardGroupCounts.Contains(2)) { return HandType.OnePair; }
            else { return HandType.HighCard; }
        }

        private static HandType FindBestJokerHandType(List<Card> cards)
        {
            var baseHandString = new string(cards.Select(c => c.Display).ToArray());
            var jokerIndexes = baseHandString.AllIndexesOf("*", StringComparison.OrdinalIgnoreCase).ToArray();
            var jokerSubstitutes = CardLookup.Keys.Where(k => k != '*').ToArray();
            var potentialHands = new List<string> { baseHandString };
            
            foreach (var jokerIndex in jokerIndexes)
            {
                var newPotentialHands = new List<string>();
                foreach (var jokerSubstitute in jokerSubstitutes)
                {
                    potentialHands.ForEach(ph => newPotentialHands.Add(ph.ReplaceAt(jokerIndex, jokerSubstitute)));
                }
                potentialHands = newPotentialHands;
            }
            
            var bestPotentialHand = potentialHands.Select(h => new Hand(h)).OrderByDescending(h => h).First();
            return bestPotentialHand.HandType;
        }

        public int CompareTo(Hand? other)
        {
            if (other == null) return 1;
            // first compare on hand type
            if (HandType != other.HandType) return HandType.CompareTo(other.HandType);
            // then compare on card strength, in order left to right
            for (int i = 0; i < 5; i++)
            {
                if (Cards[i].Strength != other.Cards[i].Strength) return Cards[i].Strength.CompareTo(other.Cards[i].Strength);
            }
            // everything equal?
            return 0;
        }
    }

    public enum HandType // (int values denote the value of each type)
    {
        FiveOfAKind = 7,
        FourOfAKind = 6,
        FullHouse = 5,
        ThreeOfAKind = 4,
        TwoPair = 3,
        OnePair = 2,
        HighCard = 1
    }
    
    public static Dictionary<char, Card> CardLookup = new Dictionary<char, Card>()
    {
        { 'A', new Card('A', 14) },
        { 'K', new Card('K', 13) },
        { 'Q', new Card('Q', 12) },
        { 'J', new Card('J', 11) },
        { 'T', new Card('T', 10) },
        { '9', new Card('9', 9) },
        { '8', new Card('8', 8) },
        { '7', new Card('7', 7) },
        { '6', new Card('6', 6) },
        { '5', new Card('5', 5) },
        { '4', new Card('4', 4) },
        { '3', new Card('3', 3) },
        { '2', new Card('2', 2) },
        { '*', new Card('*', 1) } // (Joker)
    };

    public record HandAndBid(Hand Hand, long Bid);
}

