using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day22;

public class SandSlabs : BaseDayModule
{
    public SandSlabs(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 22;
    public override string Title => "Sand Slabs";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(5);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(7);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));
    
    [Theory]
    [ClassData(typeof(XyFootprintOverlapsTestData))]
    public void XyFootprint_Overlaps(XyFootprint footprint1, XyFootprint footprint2, bool expected)
    {
        WriteLine($"Footprint 1: ({footprint1.X1},{footprint1.Y1}) ({footprint1.X2},{footprint1.Y2})");
        WriteLine($"Footprint 2: ({footprint2.X1},{footprint2.Y1}) ({footprint2.X2},{footprint2.Y2})");
        footprint1.Overlaps(footprint2).Should().Be(expected);
        footprint2.Overlaps(footprint1).Should().Be(expected);
    }

    public long ExecutePart1(string data)
    {
        var bricks = ParseBricks(data);
        WriteLine($"Part 1 - Loaded {bricks.Count} bricks");
        
        var settledBricks = SettleBricks(bricks);

        var removableBricks = CountRemovableBricks(settledBricks.settledBricks);
        
        WriteLine($"Removable Bricks: {removableBricks}");
        return removableBricks;
    }

    public long ExecutePart2(string data)
    {
        var allBricksSettled = SettleBricks(ParseBricks(data)).settledBricks;
        WriteLine($"Part 2 - Loaded {allBricksSettled.Count} bricks");
        
        var allBrickIds = allBricksSettled.Select(b => b.Id).ToList();
        var totalFallingBricks = 0;
        foreach (var brickIdToRemove in allBrickIds)
        {
            // remove one brick and re-settle everything to see how many fall.
            // could probably be more efficient, but this works to solve the puzzle.
            var bricksAfterRemoval = allBricksSettled.Where(b => b.Id != brickIdToRemove).ToList();
            // see how many bricks fall now with that one removed
            var bricksThatFall = SettleBricks(bricksAfterRemoval).fallenBricks;
            totalFallingBricks += bricksThatFall;
        }
        
        WriteLine($"Total Falling Bricks: {totalFallingBricks}");
        return totalFallingBricks;
    }
    
    private (List<Brick> settledBricks, int fallenBricks) SettleBricks(List<Brick> bricks)
    {
        var unsettledBricks = new Queue<Brick>(bricks.OrderBy(b => b.MinZ).ThenBy(b => b.MinX).ThenBy(b => b.MinY));
        var settledBricks = new List<Brick>();
        int fallenBricks = 0;
        
        while (unsettledBricks.Any())
        {
            var currentBrick = unsettledBricks.Dequeue().Copy(); // new brick instance so we don't modify the input bricks
            if (currentBrick.MinZ > 1) // (bricks already sitting at 1 are on the ground)
            {
                var settledBricksUnderThisBrick = settledBricks.Where(b => b.Footprint.Overlaps(currentBrick.Footprint)).ToList();
                var highestValueBelow = settledBricksUnderThisBrick.Any() ? settledBricksUnderThisBrick.Max(b => b.MaxZ) : 0;
                var distanceToFall = currentBrick.MinZ - highestValueBelow - 1;
                currentBrick.Move(0, 0, -distanceToFall);
                if (distanceToFall > 0)
                {
                    fallenBricks++;
                }
            }
            
            settledBricks.Add(currentBrick);
        }

        return (settledBricks, fallenBricks);
    }
    
    private int CountRemovableBricks(List<Brick> settledBricks)
    {
        int removableBricksCount = 0;
        foreach (var currentBrick in settledBricks)
        {
            var bricksDirectlyOnTopOfMe = GetBricksDirectlyOnTopOf(currentBrick, settledBricks);
            if (bricksDirectlyOnTopOfMe.Count == 0)
            {
                // no bricks on top of me, so I can be removed
                removableBricksCount++;
                continue;
            }
            // I have bricks on top of me, will they fall if I'm removed?
            if (WillBricksFallIfRemoved(currentBrick, settledBricks) == false)
            {
                removableBricksCount++;
            }
        }

        return removableBricksCount;
    }
    
    public List<Brick> GetBricksDirectlyOnTopOf(Brick brick, List<Brick> allSettledBricks)
    {
        return allSettledBricks.Where(b => b.MinZ == (brick.MaxZ + 1) && b.Footprint.Overlaps(brick.Footprint)).ToList();
    }
    
    public bool WillBricksFallIfRemoved(Brick brickToRemove, List<Brick> allSettledBricks)
    {
        var bricksDirectlyOnTopOfMe = GetBricksDirectlyOnTopOf(brickToRemove, allSettledBricks);
        if (bricksDirectlyOnTopOfMe.Count == 0)
        {
            // no bricks on top of me, so I can be removed
            return false;
        }
        
        foreach (var brickAboveMe in bricksDirectlyOnTopOfMe)
        {
            var otherSupportingBricks = GetBricksDirectlyUnderneath(brickAboveMe, allSettledBricks)
                .Where(b => b.Id != brickToRemove.Id).ToList();
            if (otherSupportingBricks.Count == 0)
            {
                // this "brick above me" has no other support, so it will fall
                return true;
            }
        }
        
        return false;
    }
    
    public List<Brick> GetBricksDirectlyUnderneath(Brick brick, List<Brick> allSettledBricks)
    {
        return allSettledBricks.Where(b => b.MaxZ == (brick.MinZ - 1) && b.Footprint.Overlaps(brick.Footprint)).ToList();
    }

    public record XyzCoordinate(int X, int Y, int Z);
    
    public class XyFootprint
    {
        public XyFootprint(int x1, int y1, int x2, int y2)
        {
            X1 = Math.Min(x1, x2);
            Y1 = Math.Min(y1, y2);
            X2 = Math.Max(x1, x2);
            Y2 = Math.Max(y1, y2);
        }

        public bool Overlaps(XyFootprint other)
        {
            return X1 <= other.X2 && X2 >= other.X1 && Y1 <= other.Y2 && Y2 >= other.Y1;
        }

        public int X1 { get; init; }
        public int Y1 { get; init; }
        public int X2 { get; init; }
        public int Y2 { get; init; }
    }

    public class Brick(XyzCoordinate corner1, XyzCoordinate corner2, string id)
    {
        public string Id { get; } = id;
        public int MinX => Math.Min(corner1.X, corner2.X);
        public int MaxX => Math.Max(corner1.X, corner2.X);
        public int MinY => Math.Min(corner1.Y, corner2.Y);
        public int MaxY => Math.Max(corner1.Y, corner2.Y);
        public int MinZ => Math.Min(corner1.Z, corner2.Z);
        public int MaxZ => Math.Max(corner1.Z, corner2.Z);
        public XyFootprint Footprint => new(MinX, MinY, MaxX, MaxY);

        /// <summary>
        /// Creates a new instance of brick with the same dimensions and ID as this one.
        /// </summary>
        public Brick Copy() => new(corner1, corner2, Id);
        
        public void Move(int dX, int dY, int dZ)
        {
            corner1 = new XyzCoordinate(X: corner1.X + dX, Y: corner1.Y + dY, Z: corner1.Z + dZ);
            corner2 = new XyzCoordinate(X: corner2.X + dX, Y: corner2.Y + dY, Z: corner2.Z + dZ);
        }
    }

    private List<Brick> ParseBricks(string data)
    {
        // example line: 3,0,83~3,0,85
        var bricks = data.ToLines(true)
            .Select((line, i) =>
            {
                var coords = line.Split('~');
                var coord1 = coords[0].Split(',').Select(int.Parse).ToArray();
                var coord2 = coords[1].Split(',').Select(int.Parse).ToArray();
                
                var id = i <= 25 ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[i].ToString() : i.ToString();
                
                return new Brick(new XyzCoordinate(coord1[0], coord1[1], coord1[2]),
                    new XyzCoordinate(coord2[0], coord2[1], coord2[2]), id);
            })
            .ToList();
        return bricks;
    }
}

public class XyFootprintOverlapsTestData : TheoryData<SandSlabs.XyFootprint, SandSlabs.XyFootprint, bool>
{
    public XyFootprintOverlapsTestData()
    {
        // same rectangles but points in various orders
        Add(new (0,0,2,2), new (1,1,3,3), true);
        Add(new (2,2,0,0), new (1,1,3,3), true);
        Add(new (0,0,2,2), new (3,3,1,1), true);
        Add(new (2,2,0,0), new (3,3,1,1), true);
        
        Add(new (0,0,2,2), new (3,0,5,2), false); // touching next to each other
        
        // two rects crossing each other
        Add(new (2,0,2,10), new (0,4,5,5), true);
        
        // two rects with distance between them
        Add(new (0,0,5,5), new (10,10,20,20), false);
        
        // one rect inside another
        Add(new (0,0,10,10), new (2,2,8,8), true);
    }
}

