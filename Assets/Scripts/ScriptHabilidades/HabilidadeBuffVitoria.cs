using UnityEngine;

[CreateAssetMenu(fileName = "Habilidade Buff Vitoria", menuName = "SuperTrunfo/Habilidades/Buff Após Vitória")]
public class HabilidadeBuffVitoria : HabilidadeBase
{
    [Header("Configuração")]
    public int valorDoBuff = 100; // Quantidade de buff para a próxima carta

    public override void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo)
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        Debug.Log($"⚔️ {usuario.cardData.nomeCarta} ativou: Se vencer, a próxima carta ganhará +{valorDoBuff}!");

        // Registra a promessa no juiz (GameManager)
        if (usuario.pertenceAoJogador)
        {
            gm.promessaBuffVitoriaJogador = valorDoBuff;
        }
        else
        {
            gm.promessaBuffVitoriaOponente = valorDoBuff;
        }
    }
}