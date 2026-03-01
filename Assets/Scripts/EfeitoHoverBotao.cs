using UnityEngine;
using UnityEngine.EventSystems; // Obrigatório para detectar o mouse

public class EfeitoHoverBotao : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 posicaoOriginal;
    
    [Header("Configuração do Hover")]
    [Tooltip("Quantos pixels o botão vai subir")]
    public float distanciaSubida = 10f; 

    void Start()
    {
        // Salva a posição inicial do botão assim que o jogo começa
        posicaoOriginal = transform.localPosition;
    }

    // Chamado automaticamente quando o mouse ENTRA no botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localPosition = posicaoOriginal + new Vector3(0, distanciaSubida, 0);
    }

    // Chamado automaticamente quando o mouse SAI do botão
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localPosition = posicaoOriginal;
    }
}