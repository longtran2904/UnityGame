using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Sibling")]
public class SiblingTile : RuleTile<SiblingTile.Neighbor> {

	public List<TileBase> siblings = new List<TileBase>();

	public class Neighbor : TilingRuleOutput.Neighbor
	{
		public const int Sibling = 3;
	}

	public override bool RuleMatch(int neighbor, TileBase tile)
	{
		switch (neighbor)
		{
			case TilingRuleOutput.Neighbor.This:
                return (siblings.Contains(tile) 
                    || base.RuleMatch(neighbor, tile));
            case TilingRuleOutput.Neighbor.NotThis:
                return (!siblings.Contains(tile)
                    && base.RuleMatch(neighbor, tile));
		}
		return base.RuleMatch(neighbor, tile);
	}
}
