using UnityEngine;

[CreateAssetMenu(fileName = "Nova Carta", menuName = "Criar Carta")]
public class CardData : ScriptableObject
{
    [Header("Identidade")]
    public string nomeCarta;
    public Sprite arteCarta;

    [Header("Classificação (Calculada Automaticamente)")]
    public CardClass classe; 
    public CardType tipo;    
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

    public void CalcularClasse()
    {
        int somaAtributos = forca + magia + agilidade + inteligencia;

        if (somaAtributos >= 3600) classe = CardClass.SS;
        else if (somaAtributos >= 3000) classe = CardClass.S;
        else if (somaAtributos >= 2400) classe = CardClass.A;
        else if (somaAtributos >= 1600) classe = CardClass.B;
        else classe = CardClass.C;
    }

    // O Unity continua calculando sozinho se você alterar manualmente
    private void OnValidate()
    {
        CalcularClasse();
    }
}

// (Assumo que os enums CardClass, CardType e CardMoral já estejam declarados 
// no final deste arquivo ou em um arquivo de script separado)