using UnityEngine;

[CreateAssetMenu(fileName = "Habilidade Maldicao", menuName = "SuperTrunfo/Habilidades/Maldicao do Cemiterio")]
public class HabilidadeMaldicao : HabilidadeBase
{
    [Header("Configuração")]
    public int penalidade = 50; // Quanto o oponente vai perder a partir do próximo turno

    public override void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo)
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        Debug.Log($"💀 {usuario.cardData.nomeCarta} lançou uma praga! Quando for para o cemitério, o inimigo perderá {penalidade} nos próximos duelos.");

        // AQUI ESTÁ A MUDANÇA: 
        // Removemos o 'alvo.valorTemporarioBonus -= penalidade;' para não afetar a carta atual.

        // Apenas registramos o debuff no GameManager para os PRÓXIMOS duelos desta rodada de 5 cartas
        if (usuario.pertenceAoJogador)
        {
            // O jogador usou a carta: amaldiçoa o oponente nos próximos turnos
            gm.modificadorGlobalOponente -= penalidade;
        }
        else
        {
            // A IA usou a carta: amaldiçoa o jogador nos próximos turnos
            gm.modificadorGlobalJogador -= penalidade;
        }
    }
}