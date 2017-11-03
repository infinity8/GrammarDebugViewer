﻿using GrammarDebugViewer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace GrammarDebugViewer
{
	public class DebugFrame
	{
		public static IntPoint GridSize;

		public string Name { get; set; }
		public IntPoint Offset { get; set; }
		public DebugTile[,] Grid { get; set; }

		public List<DebugFrame> Children { get; } = new List<DebugFrame>();

		public void Parse(XElement el, Dictionary<int, XElement> entityTable)
		{
			Name = el.Attribute("N").Value;

			var gridEl = el.Element("G");
			var offsetStr = gridEl.Attribute("O").Value;
			var split = offsetStr.Split(',');
			Offset = new IntPoint(int.Parse(split[0]), int.Parse(split[1]));

			int y = 0;
			foreach (var line in gridEl.Elements())
			{
				if (Grid == null)
				{
					Grid = new DebugTile[line.Elements().Count(), gridEl.Elements().Count()];
				}

				int x = 0;
				foreach (var tileEl in line.Elements())
				{
					var active = tileEl.Element("A") != null ? true : false;
					var charStr = tileEl.Element("C").Value.Trim();
					var c = charStr.FirstOrDefault();
					if (c == '\0') c = ' ';

					Grid[x, y] = new DebugTile(active, c);

					var contentsEl = tileEl.Element("N");
					foreach (var slotEl in contentsEl.Elements())
					{
						var slotStr = slotEl.Name.ToString();
						var slotIndex = int.Parse(slotStr.Substring(1));
						var slot = DebugTile.Slots[slotIndex];

						var contentIndex = int.Parse(slotEl.Value);
						var content = entityTable[contentIndex];

						Grid[x, y].Contents[slot] = content;
					}

					x++;
				}

				y++;
			}

			var childrenEl = el.Element("H");
			if (childrenEl != null)
			{
				foreach (var childEl in childrenEl.Elements())
				{
					var child = new DebugFrame();
					child.Parse(childEl, entityTable);
					Children.Add(child);
				}
			}
		}
	}

	public class DebugTile
	{
		public enum SpaceSlot
		{
			FLOOR,
			FLOORDETAIL,
			WALL,
			WALLDETAIL,
			BELOWENTITY,
			ENTITY,
			ABOVEENTITY,
			LIGHT
		}
		public static SpaceSlot[] Slots
		{
			get
			{
				if (s_slots == null)
				{
					var slotsList = new List<SpaceSlot>();
					foreach (SpaceSlot slot in Enum.GetValues(typeof(SpaceSlot)))
					{
						slotsList.Add(slot);
					}

					s_slots = slotsList.ToArray();
				}

				return s_slots;
			}
		}
		private static SpaceSlot[] s_slots;

		public bool Active { get; set; }
		public char Char { get; set; }

		public Dictionary<SpaceSlot, XElement> Contents { get; } = new Dictionary<SpaceSlot, XElement>();

		public DebugTile(bool active, char c)
		{
			Active = active;
			Char = c;
		}
	}
}