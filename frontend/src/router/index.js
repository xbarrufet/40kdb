import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import GamesView from '../views/GamesView.vue'
import FactionView from '../views/FactionView.vue'
import CollectionsView from '../views/CollectionsView.vue'
import FactionCollectionView from '../views/FactionCollectionView.vue'
import UnitCollectionView from '../views/UnitCollectionView.vue'
import ProjectsView from '../views/ProjectsView.vue'
import ProjectDetailView from '../views/ProjectDetailView.vue'

const routes = [
  { path: '/', name: 'home', component: HomeView },
  { path: '/games', name: 'games', component: GamesView },
  { path: '/games/:gameId', name: 'game-detail', component: GamesView },
  { path: '/games/:gameId/factions/:factionId', name: 'faction', component: FactionView },
  { path: '/collections', name: 'collections', component: CollectionsView },
  { path: '/collections/factions/:factionId', name: 'faction-collection', component: FactionCollectionView },
  { path: '/collections/units/:unitId', name: 'unit-collection', component: UnitCollectionView },
  { path: '/collections/games/:gameId/factions/:factionId/units/:unitId', name: 'unit-collection-detail', component: UnitCollectionView },
  { path: '/projects', name: 'projects', component: ProjectsView },
  { path: '/projects/:id', name: 'project-detail', component: ProjectDetailView },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
