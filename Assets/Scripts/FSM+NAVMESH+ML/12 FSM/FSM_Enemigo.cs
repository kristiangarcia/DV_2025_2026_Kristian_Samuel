using System;
using UnityEngine;

namespace DAM_IA_ML
{
    public class FSM_Enemigo 
    {
        private IEstados estadoActual;

        public void ChangeState(Enemigo enemy, IEstados newState)
        {
            // SOLUCIÓN: Si ya estamos en el estado que intentamos poner, no hacemos nada.
            // Esto evita que el Exit() y Enter() se llamen en bucle infinito cada frame.
            if (estadoActual == newState) return;

            if (estadoActual != null)
                estadoActual.Exit(enemy);

            estadoActual = newState;
            
            if (estadoActual != null)
                estadoActual.Enter(enemy);
        }

        public void Update(Enemigo enemy)
        {
            if (estadoActual != null)
                estadoActual.Update(enemy);
        }
    }
}