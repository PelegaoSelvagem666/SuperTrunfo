using UnityEngine;

public abstract class HabilidadeBase : ScriptableObject
{
    public string nomeHabilidade;
    [TextArea] public string descricao;

    public abstract void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo);
}