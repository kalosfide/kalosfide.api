using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages.KeyParams
{
    /// <summary>
    /// outil du KeyParamService de T, Tvue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TVue"></typeparam>
    public interface IGéreArchive<T, TVue> where T : AKeyBase where TVue : AKeyBase
    {
        /// <summary>
        /// Ajoute une archive reprenant la clé et les champs de la donnée avec la date DateTime.Now.
        /// </summary>
        /// <param name="donnée"></param>
        void GéreAjout(T donnée);
        /// <summary>
        /// Ajoute une archive reprenant la clé et les champs de la donnée avec la date en paramétre.
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="date"></param>
        void GéreAjout(T donnée, DateTime date);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="vue"></param>
        void GéreEdite(T donnée, TVue vue);

    }

    /// <summary>
    /// outil du KeyParamService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TVue"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class GéreArchive<T, TVue, TArchive> : IGéreArchive<T, TVue> where T : AKeyBase where TVue : AKeyBase where TArchive : AKeyBase, IAvecDate
    {
        /// <summary>
        /// DbSet des données.
        /// </summary>
        protected DbSet<T> _dbSet;
        /// <summary>
        /// DbSet des archives.
        /// </summary>
        protected DbSet<TArchive> _dbSetArchive;

        /// <summary>
        /// Requête retournant les données incluant leurs archives.
        /// </summary>
        protected IIncludableQueryable<T, ICollection<TArchive>> _query;
        /// <summary>
        /// Requête retournant les archives incluant leur donnée.
        /// </summary>
        protected IIncludableQueryable<TArchive, T> _queryArchive;

        /// <summary>
        /// Fonction retournant les archives d'une donnée.
        /// </summary>
        protected Func<T, ICollection<TArchive>> _archives;
        /// <summary>
        /// Fonction retournant la donnée d'une archive
        /// </summary>
        protected Func<TArchive, T> _donnée;

        /// <summary>
        /// Gestionnaire des archives d'un type de données.
        /// </summary>
        /// <param name="dbSet">DbSet des données</param>
        /// <param name="query">requête retournant les données incluant leurs archives</param>
        /// <param name="archives">fonction retournant les archives d'une donnée</param>
        /// <param name="dbSetArchive">DbSet des archives</param>
        /// <param name="queryArchive">requête retournant les archives incluant leur donnée</param>
        /// <param name="donnée">fonction retournant la donnée d'une archive</param>
        public GéreArchive(
            DbSet<T> dbSet,
            IIncludableQueryable<T, ICollection<TArchive>> query,
            Func<T, ICollection<TArchive>> archives,
            DbSet<TArchive> dbSetArchive,
            IIncludableQueryable<TArchive, T> queryArchive,
            Func<TArchive, T> donnée
            )
        {
            _dbSet = dbSet;
            _query = query;
            _archives = archives;
            _dbSetArchive = dbSetArchive;
            _queryArchive = queryArchive;
            _donnée = donnée;
        }

        /// <summary>
        /// crée un TArchive sans clé sans Date sans valeurs du type approprié
        /// </summary>
        /// <returns></returns>
        protected abstract TArchive CréeArchive();

        protected abstract void CopieKey(T de, TArchive vers);

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
        public void GéreAjout(T donnée, DateTime date)
        {
            TArchive archive = CréeArchive();
            CopieKey(donnée, archive);
            archive.Date = date;
            CopieDonnéeDansArchive(donnée, archive);
            _dbSetArchive.Add(archive);
        }
        /// <summary>
        /// Ajoute une archive reprenant la clé et les champs de la donnée avec la date DateTime.Now.
        /// </summary>
        /// <param name="donnée"></param>
        public void GéreAjout(T donnée)
        {
            GéreAjout(donnée, DateTime.Now);
        }

        /// <summary>
        /// crée une archive sans clé reprenant tous les champs de la donnée qui sont diffèrents de ceux de la vue qui ne sont pas null
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="vue"></param>
        /// <returns>null si rien n'a changé</returns>
        protected abstract TArchive CréeArchiveDesDifférences(T donnée, TVue vue);

        /// <summary>
        /// ajoute sans sauver une archive reprenant tous les champs de la donnée qui sont diffèrents de ceux de la vue qui ne sont pas null et datée par DateTime.Now
        /// </summary>
        /// <param name="donnée"></param>
        public void GéreEdite(T donnée, TVue vue)
        {
            TArchive archive = CréeArchiveDesDifférences(donnée, vue);
            if (archive != null)
            {
                CopieKey(donnée, archive);
                archive.Date = DateTime.Now;
                _dbSetArchive.Add(archive);
            }
        }

    }

    public abstract class KeyParamService<T, TVue> : BaseService<T>, IKeyParamService<T, TVue>
        where T : AKeyBase where TVue : AKeyBase
    {

        protected DbSet<T> _dbSet;
        protected IGéreArchive<T, TVue> _géreArchive;

        protected DValideModel<T> dValideAjoute;
        public DValideModel<T> DValideAjoute()
        {
            return dValideAjoute;
        }
        protected DValideModel<T> dValideEdite;
        public DValideModel<T> DValideEdite()
        {
            return dValideEdite;
        }
        protected DValideModel<T> dValideSupprime;
        public DValideModel<T> DValideSupprime()
        {
            return dValideSupprime;
        }
    
        public abstract T CréeDonnée();

        public abstract TVue CréeVue(T donnée);

        protected abstract void CopieKey(TVue de, T vers);

        /// <summary>
        /// Copie dans la donnée les champs existants de la vue sauf la clé.
        /// </summary>
        /// <param name="de"></param>
        /// <param name="vers"></param>
        /// <returns></returns>
        protected abstract void CopieVueDansDonnée(TVue de, T vers);

        /// <summary>
        /// copie dans la donnée les champs existants de la vue ou à défaut de la donnée pour complèter sauf la clé
        /// </summary>
        /// <param name="de"></param>
        /// <param name="vers"></param>
        /// <param name="pourComplèter"></param>
        /// <returns></returns>
        protected abstract void CopieVuePartielleDansDonnée(TVue de, T vers, T pourComplèter);

        protected InclutRelations<T> _inclutRelations = null;
        protected DCréeDataAsync<T, TVue> dCréeVueAsync;

        public KeyParamService(ApplicationContext context) : base(context)
        {
        }

        protected abstract IQueryable<T> DbSetFiltré(KeyParam param);

        public T CréeDonnée(TVue vue)
        {
            T donnée = CréeDonnée();
            CopieKey(vue, donnée);
            CopieVueDansDonnée(vue, donnée);
            return donnée;
        }

        public T CréeDonnéeEditéeComplète(TVue vuePartielle, T donnéeEnregistrée)
        {
            T donnée = CréeDonnée();
            CopieKey(vuePartielle, donnée);
            CopieVuePartielleDansDonnée(vuePartielle, donnée, donnéeEnregistrée);
            return donnée;
        }

        public virtual void AjouteSansSauver(T donnée, DateTime date)
        {
            if (_géreArchive != null)
            {
                _géreArchive.GéreAjout(donnée, date);
            }
            _dbSet.Add(donnée);
        }

        public virtual void AjouteSansSauver(T donnée)
        {
            if (_géreArchive != null)
            {
                _géreArchive.GéreAjout(donnée);
            }
            _dbSet.Add(donnée);
        }
 
       public async Task<RetourDeService<T>> Ajoute(T donnée, DateTime date)
        {
            AjouteSansSauver(donnée, date);
            return await SaveChangesAsync(donnée);
        }
 
       public async Task<RetourDeService<T>> Ajoute(T donnée)
        {
            AjouteSansSauver(donnée);
            return await SaveChangesAsync(donnée);
        }

        public void EditeSansSauver(T donnée, TVue vue)
        {
            if (_géreArchive != null)
            {
                _géreArchive.GéreEdite(donnée, vue);
            }
            else
            {
                CopieVueDansDonnée(vue, donnée);
            }
            _dbSet.Update(donnée);
        }

        public async Task<RetourDeService<T>> Edite(T donnée, TVue vue)
        {
            EditeSansSauver(donnée, vue);
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
