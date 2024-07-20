using System.Collections.Generic;

public class Node {
	public int id;
	public float posX;
	public float posY;
	public Node parent;
	public List<Node> adjacentNodes;
	public List<string> tags;
	public bool walkable;
	public float distanceToTarget;
	public float cost;
	public float F;

	public Node(int ID, int posX, int posY) {
		id = ID;
		this.posX = posX;
		this.posY = posY;
		adjacentNodes = new List<Node>();
		tags = new List<string>();
		walkable = true;
	}

	public void addTag(string t) {
		if (!tags.Contains(t))
			tags.Add(t);
	}

	public void addAdjacentNode(Node _node) {
		if (_node.id == this.id)
			return;

		foreach (Node n in adjacentNodes) {
			if (n.id == _node.id) {
				return;
			}
		}

		adjacentNodes.Add(_node);
	}
}
