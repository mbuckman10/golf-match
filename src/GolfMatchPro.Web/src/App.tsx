import { Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { FluentProvider, Spinner, webLightTheme } from '@fluentui/react-components';
import { AppShell } from './components/AppShell';

const DashboardPage = lazy(() => import('./pages/DashboardPage').then(m => ({ default: m.DashboardPage })));
const CoursesPage = lazy(() => import('./pages/CoursesPage').then(m => ({ default: m.CoursesPage })));
const CourseEditorPage = lazy(() => import('./pages/CourseEditorPage').then(m => ({ default: m.CourseEditorPage })));
const PlayersPage = lazy(() => import('./pages/PlayersPage').then(m => ({ default: m.PlayersPage })));
const PlayerEditorPage = lazy(() => import('./pages/PlayerEditorPage').then(m => ({ default: m.PlayerEditorPage })));
const MatchDashboardPage = lazy(() => import('./pages/MatchDashboardPage').then(m => ({ default: m.MatchDashboardPage })));
const CreateMatchPage = lazy(() => import('./pages/CreateMatchPage').then(m => ({ default: m.CreateMatchPage })));
const MatchDetailPage = lazy(() => import('./pages/MatchDetailPage').then(m => ({ default: m.MatchDetailPage })));
const ScorecardPage = lazy(() => import('./pages/ScorecardPage').then(m => ({ default: m.ScorecardPage })));
const BetConfigPage = lazy(() => import('./pages/BetConfigPage').then(m => ({ default: m.BetConfigPage })));
const BetResultsPage = lazy(() => import('./pages/BetResultsPage').then(m => ({ default: m.BetResultsPage })));
const IndividualResultsPage = lazy(() => import('./pages/IndividualResultsPage').then(m => ({ default: m.IndividualResultsPage })));
const BestBallResultsPage = lazy(() => import('./pages/BestBallResultsPage').then(m => ({ default: m.BestBallResultsPage })));
const BestBallSummaryPage = lazy(() => import('./pages/BestBallSummaryPage').then(m => ({ default: m.BestBallSummaryPage })));
const SkinsResultsPage = lazy(() => import('./pages/SkinsResultsPage').then(m => ({ default: m.SkinsResultsPage })));
const TournamentResultsPage = lazy(() => import('./pages/TournamentResultsPage').then(m => ({ default: m.TournamentResultsPage })));

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <BrowserRouter>
        <AppShell>
          <Suspense fallback={<Spinner label="Loading page..." />}>
            <Routes>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/courses" element={<CoursesPage />} />
              <Route path="/courses/new" element={<CourseEditorPage />} />
              <Route path="/courses/:id" element={<CourseEditorPage />} />
              <Route path="/players" element={<PlayersPage />} />
              <Route path="/players/new" element={<PlayerEditorPage />} />
              <Route path="/players/:id" element={<PlayerEditorPage />} />
              <Route path="/matches" element={<MatchDashboardPage />} />
              <Route path="/matches/new" element={<CreateMatchPage />} />
              <Route path="/matches/:id" element={<MatchDetailPage />} />
              <Route path="/matches/:id/scorecard" element={<ScorecardPage />} />
              <Route path="/matches/:id/bets" element={<BetConfigPage />} />
              <Route path="/matches/:id/bets/:betConfigId/results" element={<BetResultsPage />} />
              <Route path="/matches/:id/bets/:betConfigId/individual-results" element={<IndividualResultsPage />} />
              <Route path="/matches/:id/bets/:betConfigId/bestball-results" element={<BestBallResultsPage />} />
              <Route path="/matches/:id/bets/:betConfigId/skins-results" element={<SkinsResultsPage />} />
              <Route path="/matches/:id/bets/:betConfigId/tournament-results" element={<TournamentResultsPage />} />
              <Route path="/matches/:id/bestball-summary" element={<BestBallSummaryPage />} />
            </Routes>
          </Suspense>
        </AppShell>
      </BrowserRouter>
    </FluentProvider>
  );
}

export default App;
