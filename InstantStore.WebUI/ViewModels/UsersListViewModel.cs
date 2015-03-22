using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class UsersListViewModel
    {
        private readonly IRepository repository;

        public UsersListViewModel(IRepository repository)
        {
            this.repository = repository;
            this.Initialize();
        }

        public UsersListViewModel(IRepository repository, Guid currentId)
        {
            this.repository = repository;
            this.Initialize();
            this.Current = this.AllUsers.FirstOrDefault(x => x.Id == currentId);
        }

        public IList<User> ActiveUsers { get; private set; }

        public IList<User> BlockedUsers { get; private set; }

        public IList<User> NewUsers { get; private set; }

        public IList<User> AllUsers { get; private set; }

        public User Current { get; private set; }

        private void Initialize()
        {
            this.AllUsers = this.repository.GetUsers(x => !x.IsAdmin).ToList();
            this.NewUsers = this.AllUsers.Where(x => !x.IsActivated).ToList();
            this.BlockedUsers = this.AllUsers.Where(x => x.IsBlocked).ToList();
            this.ActiveUsers = this.AllUsers.Where(x => !x.IsBlocked && x.IsActivated).ToList();
        }
    }
}