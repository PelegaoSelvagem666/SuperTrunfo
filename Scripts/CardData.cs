using UnityEngine;

[CreateAssetMenu(fileName = "NovaCarta", menuName = "CardGame/Carta")]
public class CardData : ScriptableObject
{
    [Header("Identidade")]
    public string nomeCarta;
    public Sprite arteCarta;
    [TextArea(3, 5)] public string descricaoHabilidade;

    [Header("Classificação")]
    public CardClass classe;
    private CardType tipo;
    public CardMoral moral; 

    [Header("Atributos (0 - 1000)")]
    [Range(0, 1000)] public int forca;
    [Range(0, 1000)] public int magia;
    [Range(0, 1000)] public int agilidade;
    [Range(0, 1000)] public int inteligencia;

    public CardType Tipo { get => Tipo1; set => Tipo1 = value; }
    public CardType Tipo1 { get => tipo; set => tipo = value; }

    public int GetAttributeValue(CardAttribute atributo)
    {
        // ... (seu código de retorno de valor)
        return 0; // Exemplo simplificado
    }
}