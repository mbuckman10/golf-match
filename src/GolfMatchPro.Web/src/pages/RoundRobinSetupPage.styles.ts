import { makeStyles } from '@fluentui/react-components';

export const styles = makeStyles({
  container: {
    padding: '24px',
    maxWidth: '1200px',
    margin: '0 auto'
  },
  content: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px'
  },
  actions: {
    display: 'flex',
    gap: '12px',
    marginTop: '24px'
  }
});
