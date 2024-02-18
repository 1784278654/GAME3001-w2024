using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Color = UnityEngine.Color;

public enum TileType : int
{
    GRASS,
    WATER,
    MUD,
    STONE
}

public class Grid : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    List<List<GameObject>> grid = new List<List<GameObject>>();
    List<List<GameObject>> neighbour = new List<List<GameObject>>();
    int rowCount = 10;      // vertical tile count
    int colCount = 20;      // horizontal tile count
    int price;
    int[,] tiles =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    void Start()
    {
        //float xStart = -colCount / 2.0f + 0.5f;    // left (-x)
        //float yStart = -rowCount / 2.0f + 0.5f;    // bottom (-y)
        float xStart = 0.0f + 0.5f;    // left (-x)
        float yStart = 0.0f + 0.5f;    // bottom (-y)
        float x = xStart;
        float y = yStart;

        for (int row = 0; row < rowCount; row++)
        {
            // Allocate space for each incoming row (a row is a collection of columns)
            grid.Add(new List<GameObject>());
            for (int col = 0; col < colCount; col++)
            {
                // For every column in the row, we must create a tile and then store it!
                GameObject tile = Instantiate(tilePrefab);
                tile.transform.position = new Vector3(x, y);
                x += 1.0f;              // Step 1 unit right each iteration
                grid[row].Add(tile);    // Store the resultant tile in the 2D grid list
            }
            // Reset column position and increment row position when current row finishes
            x = xStart;
            y += 1.0f;
        }

        //Testing Neighbour function
        neighbour = GetGridNeighbours(grid, 1, 1);
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Debug.Log(neighbour[row][col].transform.position);
            }
        }

        //Testing Cost function
        float[,] cost = Cost(grid);
        for(int row = 0;row < 10; row++)
        {
            string s = "";
            for(int col = 0;col < 20; col++)
            {
                s += cost[row, col] + " ";
            }
            Debug.Log(s);
        }
    }

    void ColorGrid()
    {
        // Make increasingly red as we move right, make increasingly green as we move up
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                GameObject tile = grid[row][col];
                Vector2 position = tile.transform.position;

                // Normalize position to render as color
                position = new Vector2(position.x / 20.0f, position.y / 10.0f);
                tile.GetComponent<SpriteRenderer>().color = new Color(position.x, position.y, 0.0f, 1.0f);

                Color color = Color.white;
                switch ((TileType)tiles[row, col])
                {
                    case TileType.GRASS:
                        color = Color.green;
                        break;

                    case TileType.WATER:
                        color = Color.blue;
                        break;

                    case TileType.MUD:
                        color = Color.red;
                        break;

                    case TileType.STONE:
                        color = Color.grey;
                        break;
                }
                tile.GetComponent<SpriteRenderer>().color = color;
            }
        }

        // World to grid (quantization) test
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = WorldToGrid(mouse);
        grid[cell.y][cell.x].GetComponent<SpriteRenderer>().color = Color.magenta;

        // Grid to world (localization) test
        //GameObject selected = Instantiate(tilePrefab);
        //selected.transform.position = GridToWorld(cell);
        //selected.GetComponent<SpriteRenderer>().color = Color.magenta;
    }

    Vector2Int WorldToGrid(Vector2 position)
    {
        Vector2Int cell = new Vector2Int((int)position.x, (int)position.y);
        cell.x = Mathf.Clamp(cell.x, 0, colCount - 1);
        cell.y = Mathf.Clamp(cell.y, 0, rowCount - 1);
        return cell;
    }

    Vector2 GridToWorld(Vector2Int cell)
    {
        cell.x = Mathf.Clamp(cell.x, 0, colCount - 1);
        cell.y = Mathf.Clamp(cell.y, 0, rowCount - 1);
        return new Vector2(cell.x + 0.5f, cell.y + 0.5f);
    }

    void Update()
    {
        ColorGrid();
    }

    List<List<GameObject>> GetGridNeighbours(List<List<GameObject>> grid, int row, int column)
    {
        //this returns a 3x3 grid with the original grid itself, slightly different but works nontheless
        //row/column are all index, not starting with 1. [0,0] is first element
        List<List<GameObject>> neighbours = new List<List<GameObject>>();
        int newRow;
        int newCol;

        //left right up down diagonals
        for (int r = -1; r < 2; r++)
        {
            neighbours.Add(new List<GameObject>());
            for (int c = -1; c < 2; c++)
            {
                newRow = row + r;
                newCol = column + c;
                if (newRow >= 0 && newRow < grid.Count && newCol >= 0 && newCol < grid[newRow].Count)
                {
                    neighbours[r + 1].Add(grid[newRow][newCol]);
                }
                else
                {
                    // If the position is out of bounds, add null to indicate there's no neighbor
                    neighbours[r + 1].Add(null);
                }
            }
        }
        
        return neighbours;
    }

    float[,] Cost(List<List<GameObject>> grid)
    {
        //not sure the exact meaning of the cost thing, or the definition. Terriblly explained in instruction with few description
        float[,] cost = new float[10,20];
        //goal tile is [10, 20]
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                switch ((TileType)tiles[row, col])
                {
                    case TileType.GRASS:
                        price = 5;
                        break;

                    case TileType.WATER:
                        price = 1; 
                        break;

                    case TileType.MUD:
                        price = 10; 
                        break;

                    case TileType.STONE:
                        price = 20; 
                        break;
                }
                cost[row, col] = price + Mathf.Sqrt((10f - row)*(10f - row) + (20f - col)* (20f - col));
            }
        }
        return cost;
    }
}
