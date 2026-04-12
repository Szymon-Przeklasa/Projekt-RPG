using Godot;
using System;

public partial class TileMigrator : Node
{
	[Export] public TileMapLayer SourceMap;   // Map (ze wszystkim)
	[Export] public TileMapLayer TargetMap;   // MapBorder (tylko drzewa)
	
	// ID source setu drzew w TileSet - sprawdź w edytorze TileSet
	[Export] public int TreeSourceId = 0;
	
	// Alternatywnie: ID atlasu tile drzew (jeśli używasz Atlas)
	// Wstaw koordinaty tile które są drzewami
	[Export] public Vector2I TreeAtlasMin = new Vector2I(19, 24);
	[Export] public Vector2I TreeAtlasMax = new Vector2I(19, 24);

	public override void _Ready()
	{
		int count = 0;
		foreach (Vector2I cell in SourceMap.GetUsedCells())
		{
			int sourceId = SourceMap.GetCellSourceId(cell);
			Vector2I atlas = SourceMap.GetCellAtlasCoords(cell);
			
			// Kopiuj tylko jeśli to tile drzewa
			if (sourceId == TreeSourceId &&
				atlas.X >= TreeAtlasMin.X && atlas.X <= TreeAtlasMax.X &&
				atlas.Y >= TreeAtlasMin.Y && atlas.Y <= TreeAtlasMax.Y)
			{
				int altTile = SourceMap.GetCellAlternativeTile(cell);
				TargetMap.SetCell(cell, sourceId, atlas, altTile);
				count++;
			}
		}
		
		GD.Print($"[TileMigrator] Skopiowano {count} tile drzew na MapBorder");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
