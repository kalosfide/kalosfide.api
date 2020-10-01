using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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
        /// ajoute 
        /// </summary>
        /// <param name="donnée"></param>
        void GèreAjout(T donnée);

        void GèreEdite(T donnée, TVue vue);

        Task SupprimeArchives(T donnée);

    }

    /// <summary>
    /// outil du KeyParamService de T, Tvue,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TVue"></typeparam>
    /// <typeparam name="TArchive"></typeparam>
    public abstract class GéreArchive<T, TVue, TArchive> : IGéreArchive<T, TVue> where T : AKeyBase where TVue : AKeyBase where TArchive : AKeyBase, IKeyArchive
    {

        protected DbSet<T> _dbSet;
        protected DbSet<TArchive> _dbSetArchive;

        public GéreArchive(DbSet<T> dbSet, DbSet<TArchive> dbSetArchive)
        {
            _dbSet = dbSet;
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
        /// crée une archive sans clé reprenant toute la donnée et datée par la date
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        protected TArchive CréeArchiveComplet(T donnée, DateTime date)
        {
            TArchive archive = CréeArchive();
            archive.CopieKey(donnée.KeyParam);
            archive.Date = date;
            CopieDonnéeDansArchive(donnée, archive);
            return archive;
        }

        /// <summary>
        /// ajoute sans sauver une archive reprenant toute la donnée et datée par DateTime.Now
        /// </summary>
        /// <param name="donnée"></param>
        public void GèreAjout(T donnée)
        {
            TArchive archive = CréeArchiveComplet(donnée, DateTime.Now);
            _dbSetArchive.Add(archive);
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
        public void GèreEdite(T donnée, TVue vue)
        {
            TArchive archive = CréeArchiveDesDifférences(donnée, vue);
            if (archive != null)
            {
                archive.CopieKey(donnée.KeyParam);
                archive.Date = DateTime.Now;
                _dbSetArchive.Add(archive);
            }
        }

        protected abstract Task<List<TArchive>> Archives(T donnée); 

        public async Task SupprimeArchives(T donnée)
        {
            _dbSetArchive.RemoveRange(await Archives(donnée));
        }
    }

    public abstract class KeyParamService<T, TVue, TParam> : BaseService<T>, IKeyParamService<T, TVue, TParam>
        where T : AKeyBase where TVue : AKeyBase where TParam : KeyParam
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

        /// <summary>
        /// copie dans la donnée les champs existants de la vue sauf la clé
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="vue"></param>
        /// <returns></returns>
        public abstract void CopieVueDansDonnée(T donnée, TVue vue);

        /// <summary>
        /// copie dans la donnée les champs existants de la vue ou à défaut de la donnée pour complèter sauf la clé
        /// </summary>
        /// <param name="donnée"></param>
        /// <param name="vue"></param>
        /// <param name="donnéePourComplèter"></param>
        /// <returns></returns>
        public abstract void CopieVuePartielleDansDonnée(T donnée, TVue vue, T donnéePourComplèter);

        protected InclutRelations<T> _inclutRelations = null;
        protected DCréeDataAsync<T, TVue> dCréeVueAsync;

        public KeyParamService(ApplicationContext context) : base(context)
        {
        }

        protected abstract IQueryable<T> DbSetFiltré(KeyParam param);

        public T CréeDonnée(TVue vue)
        {
            T donnée = CréeDonnée();
            donnée.CopieKey(vue.KeyParam);
            CopieVueDansDonnée(donnée, vue);
            return donnée;
        }

        public T CréeDonnéeEditéeComplète(TVue vuePartielle, T donnéeEnregistrée)
        {
            T donnée = CréeDonnée();
            donnée.CopieKey(vuePartielle.KeyParam);
            CopieVuePartielleDansDonnée(donnée, vuePartielle, donnéeEnregistrée);
            return donnée;
        }

        public virtual void AjouteSansSauver(T donnée)
        {
            if (_géreArchive != null)
            {
                _géreArchive.GèreAjout(donnée);
            }
            _dbSet.Add(donnée);
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
                _géreArchive.GèreEdite(donnée, vue);
            }
            else
            {
                CopieVueDansDonnée(donnée, vue);
            }
            _dbSet.Update(donnée);
        }

        public async Task<RetourDeService<T>> Edite(T donnée, TVue vue)
        {
            EditeSansSauver(donnée, vue);
            return await SaveChangesAsync(donnée);
        }

        public async Task SupprimeSansSauver(T donnée)
        {
            _dbSet.Remove(donnée);
            if (_géreArchive != null)
            {
                await _géreArchive.SupprimeArchives(donnée);
            }
        }

        public async Task<RetourDeService<T>> Supprime(T donnée)
        {
            await SupprimeSansSauver(donnée);
            return await SaveChangesAsync(donnée);
        }

        public async Task<T> Lit(TParam param)
        {
            return await DbSetFiltré(param).FirstOrDefaultAsync();
        }

        public async Task<T> Lit(TParam param, InclutRelations<T> inclutRelations)
        {
            IQueryable<T> ts = inclutRelations(DbSetFiltré(param));
            return await ts.FirstOrDefaultAsync();
        }

        public virtual async Task<TVue> LitVue(TParam param)
        {
            T t = await Lit(param, _inclutRelations);
            return CréeVue(t);
        }

        protected virtual async Task<List<TVue>> CréeVues(TParam param)
        {
            IQueryable<T> ts = DbSetFiltré(param);
            IQueryable<T> tsComplets = _inclutRelations == null ? ts : _inclutRelations(ts);
            List<T> données = await tsComplets.ToListAsync();

            List<TVue> vues = null;
            if (dCréeVueAsync != null)
            {
                vues = (await Task.WhenAll(données.Select(t => dCréeVueAsync(t)))).ToList();
            }
            else
            {
                vues = données.Select(t => CréeVue(t)).ToList();
            }

            return vues;
        }

        public async Task<List<TVue>> ListeVue(TParam param)
        {
            return await CréeVues(param);
        }
    }
}
