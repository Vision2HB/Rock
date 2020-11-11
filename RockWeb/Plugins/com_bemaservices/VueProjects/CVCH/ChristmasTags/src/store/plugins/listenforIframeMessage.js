export default store => {
  window.addEventListener('message', async e => {
    
    if (e.data.event === 'transactioncomplete') {
      console.log(e.data.data)
      await store.commit('updateFinancialData',e.data.data)
      store.dispatch('processTags')
    }
  });
};


