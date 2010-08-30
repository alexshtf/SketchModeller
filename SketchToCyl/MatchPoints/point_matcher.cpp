#include "point_matcher.h"
#include "glpk.h"
#include "matrix.h"

struct VertexData
{
	int set;
};

struct ArcData
{
	double cost;
	int x;
};

inline ArcData* arcData(glp_arc* arc)
{
	return (ArcData *)(arc->data);
}

inline VertexData* vertexData(glp_vertex* v)
{
	return (VertexData *)(v->data);
}

/** Given two sets I={1,..,n} and J = {1,...,m} and the cost of matching element of I to element of J
*   in the matrix w, finds a match of size matchesCount with minimum cost, defined as a sum of costs 
*   w[i][j] for each matches i and j. Returns a matrix of size n x m, allocated by this function. The user is
*   responsible for freeing the allocated memory.
*/
Matrix<int> find_matches(const Matrix<double>& w)
{
	glp_graph* graph = glp_create_graph(sizeof(VertexData), sizeof(ArcData));
	glp_add_vertices(graph, w.rows() + w.cols());

	for(int i = 1; i <= w.rows(); ++i)
		vertexData(graph->v[i])->set = 0; // first assignment set

	for(int i = w.rows() + 1; i <= w.rows() + w.cols(); ++i)
		vertexData(graph->v[i])->set = 1; // second assignment set

	Matrix<glp_arc*> arcs(w.rows(), w.cols());
	for(int i = 1; i <= arcs.rows(); ++i)
	{
		for(int j = 1; j <= arcs.cols(); ++j)
		{
			int idx1 = i;
			int idx2 = arcs.rows() + j;
			glp_arc* arc = glp_add_arc(graph, idx1, idx2);

			arcData(arc)->cost = (int)(1000000 / w(i-1, j-1));
			arcs(i-1, j-1) = arc;
		}
	}

	int status = glp_check_asnprob(graph, offsetof(VertexData, set));
	int okalg_status = glp_asnprob_okalg(GLP_ASN_MMP, graph, offsetof(VertexData, set), offsetof(ArcData, cost), NULL, offsetof(ArcData, x));

	Matrix<int> results(arcs.rows(), arcs.cols());
	for(int i = 0; i < results.rows(); ++i)
		for(int j = 0; j < results.cols(); ++j)
			results(i, j) = arcData(arcs(i, j))->x;

	return results;
}