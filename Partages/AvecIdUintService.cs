using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    /// <summary>
    /// outil du KeyParamService de T, Tvue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEdite"></typeparam>
    public interface IAvecIdUintGèreArchive<T, TEdite> where T : AvecIdUint where TEdite : AvecIdUint
    {
        /// <summary>
        /// Ajoute une archive reprenant la clé et les champs de la donnée avec la date en paramétre.
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="date"></param>
        void GèreAjout(T donnée, DateTime date);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="vue"></param>
        void GèreEdite(T donnée, TEdite vue);

    }

    /// <summary>
    /// outil du KeyParamService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEdite"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class AvecIdUintGèreArchive<T, TEdite, TArchive> : IAvecIdUintGèreArchive<T, TEdite>
        where T : AvecIdUint where TEdite : AvecIdUint where TArchive : AvecIdUint, IAvecDate
    {
        /// <summary>
        /// DbSet des archives.
        /// </summary>
        protected DbSet<TArchive> _dbSetArchive;

        /// <summary>
        /// Gestionnaire des archives d'un type de données.
        /// </summary>
        /// <param name="dbSetArchive">DbSet des archives</param>
        public AvecIdUintGèreArchive(DbSet<TArchive> dbSetArchive)
        {
            _dbSetArchive = dbSetArchive;
        }

        /// <summary>
        /// crée un TArchive sans clé sans Date sans valeurs du type approprié
        /// </summary>
        /// <returns></returns>
        protected abstract TArchive CréeArchive();

        /// <summary>
        /// copie tous les champs sauf la clé de la donnée vers l'archive
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="archive"></param>
        protected abstract void CopieDonnéeDansArchive(T donnée, TArchive archive);

        /// <summary>
        /// Ajoute une archive reprenant la clé et les champs de la donnée avec la date en paramétre.
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="date"></param>
        public virtual void GèreAjout(T donnée, DateTime date)
        {
            TArchive archive = CréeArchive();
            archive.Id = donnée.Id;
            archive.Date = date;
            CopieDonnéeDansArchive(donnée, archive);
            _dbSetArchive.Add(archive);
        }

        /// <summary>
        /// crée une archive sans clé reprenant tous les champs de la donnée qui sont diffèrents de ceux de la vue qui ne sont pas null
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="vue"></param>
        /// <returns>null si rien n'a changé</returns>
        protected abstract TArchive CréeArchiveDesDifférences(T donnée, TEdite vue);

        /// <summary>
        /// ajoute sans sauver une archive reprenant tous les champs de la donnée qui sont diffèrents de ceux de la vue qui ne sont pas null et datée par DateTime.Now
        /// </summary>
        /// <param name="donnée"></param>
        public void GèreEdite(T donnée, TEdite vue)
        {
            TArchive archive = CréeArchiveDesDifférences(donnée, vue);
            if (archive != null)
            {
                archive.Id = donnée.Id;
                archive.Date = DateTime.Now;
                _dbSetArchive.Add(archive);
            }
        }

    }

    /// <summary>
    /// Service CRUD de base.
    /// </summary>
    /// <typeparam name="T">Entité de la base de donnée</typeparam>
    /// <typeparam name="TAjout">Objet sans Id pour ajouter à la base de donnée</typeparam>
    /// <typeparam name="TAjouté">Objet avec Id à retourner après un ajout à la base de donnée</typeparam>
    /// <typeparam name="TEdite">Objet avec Id et les champs éditables nullable</typeparam>
    public abstract class AvecIdUintService<T, TAjout, TAjouté, TEdite> : BaseService<T>, IAvecIdUintService<T, TAjout, TAjouté, TEdite>
         where T : AvecIdUint where TAjouté : AvecIdUint where TEdite : AvecIdUint
    {

        protected DbSet<T> _dbSet;
        protected IAvecIdUintGèreArchive<T, TEdite> _gèreArchive;

        protected DAvecIdUintValideModel<T> dValideAjoute;
        public DAvecIdUintValideModel<T> DValideAjoute()
        {
            return dValideAjoute;
        }
        protected DAvecIdUintValideModel<T> dValideEdite;
        public DAvecIdUintValideModel<T> DValideEdite()
        {
            return dValideEdite;
        }
        protected DAvecIdUintValideModel<T> dValideSupprime;
        public DAvecIdUintValideModel<T> DValideSupprime()
        {
            return dValideSupprime;
        }
    
        public abstract T CréeDonnée();

        protected abstract TAjouté Ajouté(T donnée, DateTime date);

        /// <summary>
        /// Copie dans la donnée tous les champs sauf l'Id.
        /// </summary>
        /// <param name="de"></param>
        /// <param name="vers"></param>
        /// <returns></returns>
        protected abstract void CopieAjoutDansDonnée(TAjout de, T vers);

        /// <summary>
        /// Copie dans la donnée les champs existants de la vue sauf l'Id.
        /// </summary>
        /// <param name="de"></param>
        /// <param name="vers"></param>
        /// <returns></returns>
        protected abstract void CopieEditeDansDonnée(TEdite de, T vers);

        /// <summary>
        /// copie dans la donnée les champs existants de la vue ou à défaut de la donnée pour compléter sauf la clé
        /// </summary>
        /// <param name="de"></param>
        /// <param name="vers"></param>
        /// <param name="pourCompléter"></param>
        /// <returns></returns>
        protected abstract void CopieVuePartielleDansDonnée(TEdite de, T vers, T pourCompléter);

        protected AvecIdUintService(ApplicationContext context) : base(context)
        {
        }

        protected async Task<uint> PremièreIdLibre()
        {
            uint[] ids = await _dbSet.Select(u => u.Id).OrderBy(id => id).ToArrayAsync();
            int nb = ids.Length;
            // si la première valeur (d'index 1 - 1) est 1, 1 n'est pas libre
            // si la première valeur (d'index 1 - 1) n'est pas 1, 1 est libre
            // si la valeur suivante (d'index 2 - 1) est 2, 2 n'est pas libre
            // si la valeur suivante (d'index 2 - 1) n'est pas 2, 2 est libre
            // etc.
            // la première valeur libre suit la dernière valeur d'index égal à valeur - 1
            uint id = 1;
            for (; id <= nb && id == ids[id - 1]; id++)
            {
            }
            return id;
        }

        public T CréeDonnéeEditéeComplète(TEdite vuePartielle, T donnéeEnregistrée)
        {
            T donnée = CréeDonnée();
            donnée.Id = vuePartielle.Id;
            CopieVuePartielleDansDonnée(vuePartielle, donnée, donnéeEnregistrée);
            return donnée;
        }

        public async Task<T> Lit(uint id)
        {
            return await _dbSet.Where(donnée => donnée.Id == id).FirstOrDefaultAsync();
        }

        public async Task<RetourDeService<TAjouté>> AjouteSansValider(T donnée, DateTime date)
        {
            if (_gèreArchive != null)
            {
                _gèreArchive.GèreAjout(donnée, date);
            }
            _dbSet.Add(donnée);
            return await SaveChangesAsync(Ajouté(donnée, date));
        }

        public async Task<RetourDeService<TAjouté>> AjouteSansValider(T donnée)
        {
            return await AjouteSansValider(donnée, DateTime.Now);
        }

        public async Task<RetourDeService<TAjouté>> Ajoute(TAjout ajout, ModelStateDictionary modelState, DateTime date)
        {
            T donnée = CréeDonnée();
            donnée.Id = await PremièreIdLibre();
            CopieAjoutDansDonnée(ajout, donnée);

            DAvecIdUintValideModel<T> dValideAjoute = DValideAjoute();
            if (dValideAjoute != null)
            {
                await dValideAjoute(donnée, modelState);
                if (!modelState.IsValid)
                {
                    return new RetourDeService<TAjouté>(TypeRetourDeService.ModelError);
                }
            }
            return await AjouteSansValider(donnée, date);

        }
        public async Task<RetourDeService<TAjouté>> Ajoute(TAjout ajout, ModelStateDictionary modelState)
        {
            return await Ajoute(ajout, modelState, DateTime.Now);
        }

        public async Task<RetourDeService<T>> Edite(T donnée, TEdite vue)
        {
            if (_gèreArchive != null)
            {
                _gèreArchive.GèreEdite(donnée, vue);
            }
            else
            {
                CopieEditeDansDonnée(vue, donnée);
            }
            _dbSet.Update(donnée);
            return await SaveChangesAsync(donnée);
        }

        public void SupprimeSansSauver(T donnée)
        {
            _dbSet.Remove(donnée);
        }

        public async Task<RetourDeService<T>> Supprime(T donnée)
        {
            SupprimeSansSauver(donnée);
            return await SaveChangesAsync(donnée);
        }
    }
}
