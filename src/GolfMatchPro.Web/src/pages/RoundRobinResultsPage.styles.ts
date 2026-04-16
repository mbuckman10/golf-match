import { makeStyles } from '@fluentui/react-components';

export const styles = makeStyles({
  container: {
    padding: '24px',
    maxWidth: '1200px',
    margin: '0 auto',
    display: 'flex',
    flexDirection: 'column',
    gap: '24px'
  },
  positiveAmount: {
    color: 'var(--golf-green-primary)',
    fontWeight: '600'
  },
  negativeAmount: {
    color: '#d6373f',
    fontWeight: '600'
  }
});
