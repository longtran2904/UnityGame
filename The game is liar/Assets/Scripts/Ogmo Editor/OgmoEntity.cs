using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class OgmoEntity
{
    public string name;
    public int x;
    public int y;
    public float width;
    public float height;
    public List<Vector2> nodes;
    public Dictionary<string, string> entityAttributes;
    public OgmoEntity(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        name = node.Name;
        width = float.NaN;
        height = float.NaN;
        entityAttributes = new Dictionary<string, string>();
        for (int ii = 0; ii < attributes.Count; ii++)
        {
            XmlNode attribute = attributes[ii];
            switch (attribute.Name)
            {
                case "x":
                    x = System.Convert.ToInt32(attribute.Value);
                    break;
                case "y":
                    y = System.Convert.ToInt32(attribute.Value);
                    break;
                case "width":
                    width = System.Convert.ToSingle(attribute.Value);
                    break;
                case "height":
                    height = System.Convert.ToSingle(attribute.Value);
                    break;
                default:
                    entityAttributes[attribute.Name] = attribute.Value;
                    break;
            }
        }
        nodes = new List<Vector2>();
        for (int ii = 0; ii < node.ChildNodes.Count; ii++)
        {
            XmlNode childNode = node.ChildNodes[ii];
            attributes = childNode.Attributes;
            Vector2 nodePosition = new Vector2(System.Convert.ToSingle(attributes.GetNamedItem("x").Value),
                     System.Convert.ToSingle(attributes.GetNamedItem("y").Value));
            nodes.Add(nodePosition);
        }        
    }
}