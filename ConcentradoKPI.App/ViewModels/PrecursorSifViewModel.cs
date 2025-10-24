﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.ViewModels
{
    public class PrecursorSifViewModel : INotifyPropertyChanged
    {
        public string EncabezadoDescripcion { get; }

        public ObservableCollection<PrecursorSifRecord> Registros { get; } = new();

        private PrecursorSifRecord? _seleccionado;
        public PrecursorSifRecord? Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                OnPropertyChanged();
                CargarEnFormulario(value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Formulario editable
        private PrecursorSifRecord _form = new();
        public PrecursorSifRecord Form
        {
            get => _form;
            set
            {
                if (_form == value) return;
                _form = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // UEN con los valores que pediste
        public string[] Uens { get; } =
            new[] { "CMA", "Comportamiento", "Condición", "Potencial", "Precursor", "Real" };

        // Comandos
        public ICommand AgregarCmd { get; }
        public ICommand GuardarCmd { get; }
        public ICommand EliminarCmd { get; }
        public ICommand LimpiarCmd { get; }

        public int TotalRegistros => Registros.Count;

        public PrecursorSifViewModel(string semana, string proyecto)
        {
            EncabezadoDescripcion = $"Semana {semana}  |  {proyecto}";

            AgregarCmd = new RelayCommand(_ => Agregar(), _ => PuedeAgregar());
            GuardarCmd = new RelayCommand(_ => Guardar(), _ => Seleccionado != null);
            EliminarCmd = new RelayCommand(_ => Eliminar(), _ => Seleccionado != null);
            LimpiarCmd = new RelayCommand(_ => Limpiar());

            Registros.CollectionChanged += (_, __) => OnPropertyChanged(nameof(TotalRegistros));
        }

        private bool PuedeAgregar()
        {
            // Reglas mínimas
            return !string.IsNullOrWhiteSpace(Form.NombreInvolucrado)
                && !string.IsNullOrWhiteSpace(Form.CompaniaContratista)
                && !string.IsNullOrWhiteSpace(Form.PrecursorSif); // requerido por el Excel
        }

        private void Agregar()
        {
            var nuevo = Form.Clone();
            nuevo.No = Registros.Count + 1;
            Registros.Add(nuevo);
            Limpiar();
        }

        private void Guardar()
        {
            if (Seleccionado is null) return;
            Seleccionado.CopyFrom(Form);
        }

        private void Eliminar()
        {
            if (Seleccionado is null) return;
            var idx = Registros.IndexOf(Seleccionado);
            if (idx >= 0) Registros.RemoveAt(idx);
            Reenumerar();
            Limpiar();
        }

        private void Reenumerar()
        {
            for (int i = 0; i < Registros.Count; i++)
                Registros[i].No = i + 1;
        }

        private void Limpiar()
        {
            Seleccionado = null;
            Form = new PrecursorSifRecord();
        }

        private void CargarEnFormulario(PrecursorSifRecord? r)
        {
            if (r == null) return;
            Form = r.Clone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

   
    
}
