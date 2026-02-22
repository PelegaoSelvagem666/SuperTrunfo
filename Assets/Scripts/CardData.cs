using UnityEngine;

// Definição das classes de raridade

[CreateAssetMenu(fileName = "Nova Carta", menuName = "Criar Carta")]
public class CardData : ScriptableObject
{
    [Header("Identidade")]
    public string nomeCarta;
    public Sprite arteCarta;

    [Header("Classificação")]
    public CardClass classe; 
    public CardType tipo1;   
    public CardType tipo2;   
    public CardMoral moral;

    [Header("Habilidade")]
    public string tituloHabilidade; 
    [TextArea(3, 5)] 
    public string descricaoHabilidade; 

    [Header("Atributos (0 - 1000)")]
    [Range(0, 1000)] public int forca;
    [Range(0, 1000)] public int magia;
    [Range(0, 1000)] public int agilidade;
    [Range(0, 1000)] public int inteligencia;
    
}