using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStar {
	public AStar() {

	}

	public List<Node> findPath(Node start, Node end) {
		List<Node> Path = new List<Node>();
		List<Node> OpenList = new List<Node>();
		List<Node> ClosedList = new List<Node>();
		List<Node> adjacencies;
		Node current = start;

		// add start node to Open List
		OpenList.Add(start);

		while (OpenList.Count != 0 && !ClosedList.Exists(x => x.id == end.id)) {
			current = OpenList[0];
			OpenList.Remove(current);
			ClosedList.Add(current);
			adjacencies = current.adjacentNodes;


			for (int nn = 0; nn < adjacencies.Count; nn++) {
				Node n = adjacencies[nn];

				if (!ClosedList.Contains(n))// && n.Walkable)
				{
					if (!OpenList.Contains(n)) {
						n.parent = current;
						n.cost = 1 + n.parent.cost;
						OpenList.Add(n);
						OpenList = OpenList.OrderBy(node => node.F).ToList<Node>();

						adjacencies[nn] = n;
					}
				}
			}
		}

		// construct path, if end was not closed return null
		if (!ClosedList.Exists(x => x.id == end.id)) {
			return new List<Node>();
		}

		// if all good, return path
		Node temp = ClosedList[ClosedList.IndexOf(current)];
		while (temp.parent.id != start.id && temp != null) {
			Path.Add(temp);
			temp = temp.parent;
		}
		Path.Add(temp);

		Path.Reverse(); // start is index 0

		return Path;
	}

	public static List<Vector2> nodeList2posList(List<Node> nodeList) {
		List<Vector2> posList = new List<Vector2>();

		foreach (Node n in nodeList) {
			posList.Add(new Vector2(n.posX, n.posY));
		}

		return posList;
	}
}



