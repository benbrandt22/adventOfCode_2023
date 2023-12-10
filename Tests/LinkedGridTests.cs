using Core.Shared;

namespace Tests;

public class LinkedGridTests
{
    [Fact]
    public void LinkedGridCell_findsNeighbors_as_expected()
    {
        /* 2x2 grid like this:
         * A B
         * C D
         */
        
        var grid = new LinkedGrid<string>(2, 2);
        grid[0, 0].Value = "A";
        grid[0, 1].Value = "B";
        grid[1, 0].Value = "C";
        grid[1, 1].Value = "D";
        
        grid[0, 0].NeighborUp.Should().BeNull();
        grid[0, 0].NeighborDown?.Value.Should().Be("C");
        grid[0, 0].NeighborLeft.Should().BeNull();
        grid[0, 0].NeighborRight?.Value.Should().Be("B");
        
        grid[0, 1].NeighborUp.Should().BeNull();
        grid[0, 1].NeighborDown?.Value.Should().Be("D");
        grid[0, 1].NeighborLeft?.Value.Should().Be("A");
        grid[0, 1].NeighborRight.Should().BeNull();
        
        grid[1, 0].NeighborUp?.Value.Should().Be("A");
        grid[1, 0].NeighborDown.Should().BeNull();
        grid[1, 0].NeighborLeft.Should().BeNull();
        grid[1, 0].NeighborRight?.Value.Should().Be("D");
        
        grid[1, 1].NeighborUp?.Value.Should().Be("B");
        grid[1, 1].NeighborDown.Should().BeNull();
        grid[1, 1].NeighborLeft?.Value.Should().Be("C");
        grid[1, 1].NeighborRight.Should().BeNull();
    }
    
    [Fact]
    public void AllCells_returns_all_cells_in_order()
    {
        /* 2x2 grid like this:
         * A B
         * C D
         */
        
        var grid = new LinkedGrid<string>(2, 2);
        grid[0, 0].Value = "A";
        grid[0, 1].Value = "B";
        grid[1, 0].Value = "C";
        grid[1, 1].Value = "D";
        
        grid.AllCells.Select(cell => cell.Value).Should().BeEquivalentTo("A", "B", "C", "D");
    }
}